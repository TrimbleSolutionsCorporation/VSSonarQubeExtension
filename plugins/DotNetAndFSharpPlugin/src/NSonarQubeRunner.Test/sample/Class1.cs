using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginsOptionsController
{
    class Class1
    {
        public Class1()
        {
            
        }

        internal void MethodAccessException()
        {
            this.Abcd();
        }

        private void Abcd()
        {
            // dsadas
        }
    }
}
