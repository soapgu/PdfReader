using Caliburn.Micro;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfReader.ViewModels
{
    [Export(typeof( IShell ))]
    public class ShellViewModel:Conductor<WorkSpaceViewModel>.Collection.OneActive,IShell
    {
        public void NewPdf()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "文件|*.pdf";

            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                var filePath = dialog.FileName;

                var workSpace =  IoC.Get<WorkSpaceViewModel>();
                workSpace.Initialize(filePath);
                this.ActivateItem(workSpace);
            }  
        }
    }
}
