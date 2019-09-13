using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.VModels.Manga
{
    class VManga : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaiseProperty(string name)
        {
            //System.Diagnostics.Debug.WriteLine("RaiseProperty:" + name);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public Models.Manga Manga { get; set; } = null;
    }
}
