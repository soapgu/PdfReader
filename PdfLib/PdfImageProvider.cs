/*! MoonPdfLib - Provides a WPF user control to display PDF files
Copyright (C) 2013  (see AUTHORS file)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
!*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PdfLib.MuPdf;
//using PdfLib.Virtualizing;
using PdfLib.Helper;
using System.Diagnostics;

namespace PdfLib
{
    public class PdfImageProvider : IItemsProvider<ImageSource>
	{
        private IPdfSource pdfSource;
		private int count = -1;
        private string password;

		public PageDisplaySettings Settings { get; private set; }

		public PdfImageProvider(IPdfSource pdfSource, PageDisplaySettings settings , string password = null)
		{
            this.pdfSource = pdfSource;
			this.Settings = settings;
            this.password = password;
		}

		public int FetchCount()
		{
			if (count == -1)
                count = MuPdfWrapper.CountPages(pdfSource, this.password);
			
			return count;
		}

        public ImageSource Fetch(int pageNumber)
		{

            ImageSource retValue = null;

            using (var bmp = MuPdfWrapper.ExtractPage(pdfSource, pageNumber, this.Settings.ZoomFactor, this.password))
			{
				if (Settings.Rotation != ImageRotation.None)
				{
					var flipType = System.Drawing.RotateFlipType.Rotate90FlipNone;

					if (Settings.Rotation != ImageRotation.Rotate90)
						flipType = Settings.Rotation == ImageRotation.Rotate180 ? System.Drawing.RotateFlipType.Rotate180FlipNone : System.Drawing.RotateFlipType.Rotate270FlipNone;

					bmp.RotateFlip(flipType);
				}

				var bms = bmp.ToBitmapSource();
                // Freeze bitmap to avoid threading problems when using AsyncVirtualizingCollection,
                // because FetchRange is NOT called from the UI thread
				bms.Freeze();


                retValue = bms;

				// if first page and viewtype bookview, add the first page and continue with next
			}

				// if all images per row were added or the end of the pdf is reached, add the remaining PdfImages from rowList to the final list
				
			return retValue;
		}
	}

	public enum ImageRotation
	{
		None,
		Rotate90,
		Rotate180,
		Rotate270
	}
}
