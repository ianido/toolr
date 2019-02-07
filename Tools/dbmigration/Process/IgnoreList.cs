using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbmigration.Process
{
    public class IgnoreList
    {
        private string[] _ignoreList = null;
        private bool _ignoreCase = true;
        public IgnoreList(string[] ignoreListWords, bool ignoreCase = true)
        {
            _ignoreList = ignoreListWords;
            _ignoreCase = ignoreCase;

            // Normalize list
            for (int i = 0; i < _ignoreList.Length; i++)
            {
                string citem = _ignoreList[i].Trim();
                if (ignoreCase) citem = citem.ToUpper();
                _ignoreList[i] = citem;
            }
        }

        public bool IgnoreTable(TableInfo ti)
        {
            return (_ignoreList.FirstOrDefault(
                e => e.Equals(ti.Schema + "." + ti.Name, _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                    || e.Equals("[" + ti.Schema + "]." + ti.Name, _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                    || e.Equals(ti.Schema +".[" + ti.Name + "]", _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                    || e.Equals("[" + ti.Schema + "].[" + ti.Name + "]", _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                    || e.Equals("[" + ti.Name + "]", _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                    || e.Equals(ti.Name, _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                    || e.Equals("[" + ti.Schema + "].*", _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                    || e.Equals(ti.Schema + ".*", _ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture)
                   ) != null);
        }
    }
}
