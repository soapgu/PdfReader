using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PdfReader.VO
{
    public class PageVO : PropertyChangedBase
    {
        private ImageSource image;

        private int pageNumber;

        public ImageSource Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = value;
                this.NotifyOfPropertyChange(() => this.Image);
            }
        }

        public int PageNumber
        {
            get
            {
                return this.pageNumber;
            }
            set
            {
                this.pageNumber = value;
                this.NotifyOfPropertyChange(() => this.PageNumber);
            }
        }
    }
}
