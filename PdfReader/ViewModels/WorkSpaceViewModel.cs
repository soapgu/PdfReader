using Caliburn.Micro;
using PdfLib.MuPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using PdfLib;
using PdfReader.VO;
using System.ComponentModel.Composition;
using System.IO;
using Xceed.Wpf.DataGrid;
using System.Windows.Controls;

namespace PdfReader.ViewModels
{
    [Export(typeof(WorkSpaceViewModel)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class WorkSpaceViewModel:Screen
    {
        private IItemsProvider<ImageSource> provider;

        public void Initialize(string pdfPath)
        {
            var source = new FileSource(pdfPath);
            provider = new PdfImageProvider(source, new PageDisplaySettings( ImageRotation.None , 1f ));
            FileInfo file = new FileInfo(pdfPath);
            this.DisplayName = file.Name;
        }

        private List<PageVO> fakeItems;

        public List<PageVO> FakeItems
        {
            get
            {
                return this.fakeItems;
            }
            set
            {
                this.fakeItems = value;
                this.NotifyOfPropertyChange(() => this.FakeItems);
            }
        }

        private DataGridCollectionViewBase items;

        public DataGridCollectionViewBase Items
        {
            get
            {
                return this.items;
            }
            set
            {
                this.items = value;
                this.NotifyOfPropertyChange(() => this.Items);
            }
        }

        private int totleCount;

        public int TotleCount
        {
            get
            {
                return this.totleCount;
            }
            set
            {
                this.totleCount = value;
                this.NotifyOfPropertyChange(() => this.TotleCount);
            }
        }

        private int currentPageNumber;

        public int CurrentPageNumber
        {
            get
            {
                return this.currentPageNumber;
            }
            set
            {
                if (this.currentPageNumber != value)
                {
                    this.currentPageNumber = value;
                    this.NotifyOfPropertyChange(() => this.CurrentPageNumber);
                }
            }
        }

        private DataGridVirtualizingCollectionView CreateDataSource()
        {
            var c = new DataGridVirtualizingCollectionView(typeof(PageVO), false, 5, 10);
            c.QueryItemCount += OnQueryItemCount;
            c.QueryItems += OnQueryItems;
            c.AbortQueryItems += OnAbortQueryItems;
            c.DistinctValuesConstraint = DistinctValuesConstraint.Filtered;
            c.FilterCriteriaMode = FilterCriteriaMode.None; //because we use autofilter, not manually entered
            
            return c;
        }

        private void OnQueryItemCount(object sender, QueryItemCountEventArgs e)
        {
            //var count = this.provider.FetchCount();
            e.Count = this.TotleCount;
        }

        private async void OnQueryItems(object sender, QueryItemsEventArgs e)
        {
            Console.WriteLine("Request item from{0} count {1}", e.AsyncQueryInfo.StartIndex, e.AsyncQueryInfo.RequestedItemCount);
            var data = new List<object>();
            for (int i = e.AsyncQueryInfo.StartIndex; i < e.AsyncQueryInfo.StartIndex + e.AsyncQueryInfo.RequestedItemCount; i++)
            {

                PageVO page = new PageVO();
                page.PageNumber = i + 1;
               
                var pageNumber = i + 1;
                var image = await Task.Factory.StartNew(  state => this.provider.Fetch((int)state),pageNumber );
                Console.WriteLine("Render Image " + pageNumber);
                page.Image = image;
               
                data.Add(page);
            }
            e.AsyncQueryInfo.EndQuery(data.ToArray());
        }

        private void OnAbortQueryItems(object sender, QueryItemsEventArgs e)
        {
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            this.TotleCount = this.provider.FetchCount();
            this.Items = this.CreateDataSource();
            
            /*
            var list = new List<PageVO>();
            for (int i = 0; i < 30; i++ )
            {
                PageVO page = new PageVO();
                page.PageNumber = i + 1;
                //Task.Factory.StartNew(() =>
                //{
                    var image = this.provider.Fetch(1);
                    //Execute.OnUIThread(() => page.Image = image);
                    page.Image = image;
                //});
                list.Add(page);
            }

            this.FakeItems = list;
             */
        }

        public void OnScrollChanged(ScrollChangedEventArgs e)
        {
            //Console.WriteLine("VerticalOffset:" + e.VerticalOffset);
            var currentPage = e.VerticalOffset / 1400 + 1;
            this.CurrentPageNumber = (int)currentPage;
        }
    }
}
