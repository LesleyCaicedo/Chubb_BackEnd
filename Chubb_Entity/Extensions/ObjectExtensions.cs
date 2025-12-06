using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chubb_Entity.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Mapea un solo objeto (anónimo, diccionario, o clase) a una instancia del modelo especificado.
        /// </summary>
        public static T MapToModel<T>(this object source) where T : new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            T destination = new();
            var destProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (source is IDictionary<string, object> dict)
            {
                foreach (var prop in destProps)
                {
                    if (dict.TryGetValue(prop.Name, out var value) && value != null)
                    {
                        prop.SetValue(destination, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
                return destination;
            }

            var srcProps = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in destProps)
            {
                var srcProp = Array.Find(srcProps, p => p.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                if (srcProp != null)
                {
                    var value = srcProp.GetValue(source);
                    if (value != null)
                        prop.SetValue(destination, Convert.ChangeType(value, prop.PropertyType));
                }
            }

            return destination;
        }

        /// <summary>
        /// Mapea un DataRow a un modelo del tipo especificado.
        /// </summary>
        private static T MapDataRowToModel<T>(this DataRow row) where T : new()
        {
            T obj = new();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (row.Table.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                {
                    prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                }
            }

            return obj;
        }

        /// <summary>
        /// Convierte un objeto genérico (lista, DataTable, o único objeto)
        /// en una lista de modelos del tipo especificado.
        /// </summary>
        public static List<T> MapObjectToList<T>(this object source) where T : new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Caso 1: DataTable
            if (source is DataTable table)
            {
                return table.AsEnumerable()
                            .Select(row => row.MapDataRowToModel<T>())
                            .ToList();
            }

            // Caso 2: Colección (List<object>, object[], IEnumerable)
            if (source is IEnumerable enumerable && source is not string)
            {
                var list = new List<T>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                        list.Add(item.MapToModel<T>());
                }
                return list;
            }

            // Caso 3: Objeto único
            return new List<T> { source.MapToModel<T>() };
        }
    }
}
