using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DaAPI.Core.Helper
{
    //based on https://stackoverflow.com/questions/383686/how-do-you-loop-through-currently-loaded-assemblies

    /// <summary>
    ///     Intent: Get referenced assemblies, either recursively or flat. Not thread safe, if running in a multi
    ///     threaded environment must use locks.
    /// </summary>
    public static class AssemblyHelper
    {
        //public class MissingAssembly
        //{
        //    public MissingAssembly(string missingAssemblyName, string missingAssemblyNameParent)
        //    {
        //        MissingAssemblyName = missingAssemblyName;
        //        MissingAssemblyNameParent = missingAssemblyNameParent;
        //    }

        //    public string MissingAssemblyName { get; set; }
        //    public string MissingAssemblyNameParent { get; set; }
        //}

        private static Dictionary<string, Assembly> _dependentAssemblyList;

        /// <summary>
        ///     Intent: Get assemblies referenced by entry assembly. Not recursive.
        /// </summary>
        //public static List<string> GetReferencedAssembliesFlat(this Type type)
        //{
        //    var results = type.Assembly.GetReferencedAssemblies();
        //    return results.Select(o => o.FullName).OrderBy(o => o).ToList();
        //}

        /// <summary>
        ///     Intent: Get assemblies currently dependent on entry assembly. Recursive.
        /// </summary>
        public static Dictionary<string, Assembly> GetReferencedAssembliesRecursive(
            Boolean withGAC = false, IEnumerable<Assembly> addtionalRoots = null)
        {
            _dependentAssemblyList = new Dictionary<string, Assembly>();
            //_missingAssemblyList = new List<MissingAssembly>();

            List<Assembly> rootAssemblies = new List<Assembly> {
                Assembly.GetEntryAssembly(),
                Assembly.GetExecutingAssembly(),
                Assembly.GetCallingAssembly() };

            if (addtionalRoots != null)
            {
                rootAssemblies.AddRange(addtionalRoots);
            }

            foreach (Assembly assembly in rootAssemblies)
            {
                if (assembly == null) { continue; }

                String assemblyName = assembly.FullName.GetDisplayFriendlyAssemblyName();

                if (_dependentAssemblyList.ContainsKey(assemblyName) == true) { continue; }

                _dependentAssemblyList.Add(assemblyName, assembly);
                GetDependentAssembliesRecursive(assembly);
            }

            if (withGAC == false)
            {
                // Only include assemblies that we wrote ourselves (ignore ones from GAC).
                var keysToRemove = _dependentAssemblyList.Values.Where(
                    o => o.GlobalAssemblyCache == true).ToList();

                foreach (var k in keysToRemove)
                {
                    _dependentAssemblyList.Remove(k.FullName.GetDisplayFriendlyAssemblyName());
                }
            }

            return _dependentAssemblyList;
        }

        /// <summary>
        ///     Intent: Get missing assemblies.
        /// </summary>
        //public static List<MissingAssembly> MyGetMissingAssembliesRecursive(this Assembly assembly)
        //{
        //    _dependentAssemblyList = new Dictionary<string, Assembly>();
        //    _missingAssemblyList = new List<MissingAssembly>();
        //    InternalGetDependentAssembliesRecursive(assembly);

        //    return _missingAssemblyList;
        //}

        /// <summary>
        ///     Intent: Internal recursive class to get all dependent assemblies, and all dependent assemblies of
        ///     dependent assemblies, etc.
        /// </summary>
        private static void GetDependentAssembliesRecursive(Assembly assembly)
        {
            // Load assemblies with newest versions first. Omitting the ordering results in false positives on
            // _missingAssemblyList.
            IEnumerable<AssemblyName> referencedAssemblies = assembly.GetReferencedAssemblies()
                .OrderByDescending(o => o.Version);

            foreach (AssemblyName r in referencedAssemblies)
            {
                if (String.IsNullOrEmpty(assembly.FullName))
                {
                    continue;
                }

                if (_dependentAssemblyList.ContainsKey(r.FullName.GetDisplayFriendlyAssemblyName()) == false)
                {
                    try
                    {
                        Assembly childAssembly = Assembly.Load(r);

                        _dependentAssemblyList.Add(childAssembly.FullName.GetDisplayFriendlyAssemblyName(), childAssembly);
                        GetDependentAssembliesRecursive(childAssembly);
                    }
                    catch
                    {
                        continue;
                        //_missingAssemblyList.Add(new MissingAssembly(r.FullName.Split(',')[0], assembly.FullName.MyToName()));
                    }
                }
            }
        }

        private static String GetDisplayFriendlyAssemblyName(this String fullName)
        {
            return fullName.Split(',')[0];
        }
    }
}
