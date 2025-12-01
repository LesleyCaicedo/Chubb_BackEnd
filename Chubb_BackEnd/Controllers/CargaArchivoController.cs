using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Data;
using OfficeOpenXml;

namespace Chubb_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CargaArchivoController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se envió ningún archivo.");

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".xlsx" && extension != ".xls" && extension != ".txt")
                return BadRequest("Solo se permiten archivos Excel (.xlsx/.xls) o TXT.");

            try
            {
                DataTable data;

                if (extension == ".txt")
                {
                    data = await ProcesarTxt(file);
                }
                else
                {
                    data = await ProcesarExcel(file);
                }

                // Aquí puedes guardar en DB o lo que necesites
                return Ok(new
                {
                    mensaje = "Archivo procesado con éxito",
                    filas = data.Rows.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error procesando archivo: {ex.Message}");
            }
        }

        private async Task<DataTable> ProcesarTxt(IFormFile file)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Linea");

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string linea;
                while ((linea = await reader.ReadLineAsync()) != null)
                {
                    dataTable.Rows.Add(linea);
                }
            }

            return dataTable;
        }

        private async Task<DataTable> ProcesarExcel(IFormFile file)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var dataTable = new DataTable();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets[0];
                    int rowCount = ws.Dimension.Rows;
                    int colCount = ws.Dimension.Columns;

                    // Crear columnas
                    for (int col = 1; col <= colCount; col++)
                        dataTable.Columns.Add(ws.Cells[1, col].Text.Trim());

                    // Leer filas
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var newRow = dataTable.NewRow();
                        for (int col = 1; col <= colCount; col++)
                        {
                            newRow[col - 1] = ws.Cells[row, col].Text.Trim();
                        }
                        dataTable.Rows.Add(newRow);
                    }
                }
            }

            return dataTable;
        }
    }
}
