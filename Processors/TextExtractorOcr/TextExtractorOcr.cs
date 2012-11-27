using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

namespace TextExtractorOcr
{
	class gmseDeskew
	{
		// Representation of a line in the image.
		class HougLine
		{
			// Count of points in the line.
			public int Count;
			// Index in Matrix.
			public int Index;
			// The line is represented as all x,y that solve y*cos(alpha)-x*sin(alpha)=d
			public double Alpha;
			public double d;
		}

		// The Bitmap
		Bitmap cBmp;
		// The range of angles to search for lines
		double cAlphaStart = -20;
		double cAlphaStep = 0.2;
		int cSteps = 40 * 5;
		// Precalculation of sin and cos.
		double[] cSinA;
		double[] cCosA;
		// Range of d
		double cDMin;
		double cDStep = 1;
		int cDCount;
		// Count of points that fit in a line.
		int[] cHMatrix;

		public gmseDeskew(Bitmap bmp)
		{
			cBmp = bmp;
		}

		// Calculate the skew angle of the image cBmp.
		public double GetSkewAngle()
		{
			HougLine[] hl;
			double sum = 0;
			int count = 0;

			// Hough Transformation
			Calc();
			// Top 20 of the detected lines in the image.
			hl = GetTop(20);

			// Average angle of the lines
			for (int i = 0; i < 19; i++)
			{
				sum += hl[i].Alpha;
				count++;
			}
			return sum / count;
		}

		// Calculate the Count lines in the image with most points.    
		private HougLine[] GetTop(int Count)
		{
			HougLine[] hl = new HougLine[Count];
			for (int i = 0; i < Count; i++)
			{
				hl[i] = new HougLine();
			}

			HougLine tmp;

			for (int i = 0; i < cHMatrix.Length - 1; i++)
			{
				if (cHMatrix[i] > hl[Count - 1].Count)
				{
					hl[Count - 1].Count = cHMatrix[i];
					hl[Count - 1].Index = i;
					int j = Count - 1;
					while (j > 0 && hl[j].Count > hl[j - 1].Count)
					{
						tmp = hl[j];
						hl[j] = hl[j - 1];
						hl[j - 1] = tmp;
						j--;
					}
				}
			}

			int AlphaIndex, dIndex;

			for (int i = 0; i < Count; i++)
			{
				dIndex = hl[i].Index / cSteps;
				AlphaIndex = hl[i].Index - dIndex * cSteps;
				hl[i].Alpha = GetAlpha(AlphaIndex);
				hl[i].d = dIndex + cDMin;
			}
			return hl;
		}

		// Hough Transforamtion:   
		private void Calc()
		{
			int hMin = cBmp.Height / 4;
			int hMax = cBmp.Height * 3 / 4;
			Init();

			for (int y = hMin; y < hMax; y++)
			{
				for (int x = 1; x < cBmp.Width - 2; x++)
				{
					// Only lower edges are considered.           
					if (IsBlack(x, y))
					{
						if (!IsBlack(x, y + 1))
						{
							Calc(x, y);
						}
					}
				}
			}
		}

		// Calculate all lines through the point (x,y). 
		private void Calc(int x, int y)
		{
			double d;
			int dIndex;
			int index;

			for (int alpha = 0; alpha < cSteps - 1; alpha++)
			{
				d = y * cCosA[alpha] - x * cSinA[alpha];
				dIndex = (int)CalcDIndex(d);
				index = dIndex * cSteps + alpha;
				try
				{
					cHMatrix[index] += 1;
				}
				catch (Exception)
				{
				}
			}
		}

		private double CalcDIndex(double d)
		{
			return Convert.ToInt32(d - cDMin);
		}

		private bool IsBlack(int x, int y)
		{
			Color c = cBmp.GetPixel(x, y);
			double luminance = (c.R * 0.299) + (c.G * 0.587) + (c.B * 0.114);
			return luminance < 140;
		}

		private void Init()
		{
			double angle;
			// Precalculation of sin and cos.       
			cSinA = new double[cSteps - 1];
			cCosA = new double[cSteps - 1];
			for (int i = 0; i < cSteps - 1; i++)
			{
				angle = GetAlpha(i) * Math.PI / 180.0;
				cSinA[i] = Math.Sin(angle);
				cCosA[i] = Math.Cos(angle);
			}
			// Range of d:       
			cDMin = -cBmp.Width;
			cDCount = (int)(2 * (cBmp.Width + cBmp.Height) / cDStep);
			cHMatrix = new int[cDCount * cSteps];
		}

		public double GetAlpha(int Index)
		{
			return cAlphaStart + Index * cAlphaStep;
		}

		public static Bitmap RotateImage(Bitmap bmp, double angle)
		{
			Graphics g;
			Bitmap tmp = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			tmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
			g = Graphics.FromImage(tmp);
			try
			{
				g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
				g.RotateTransform((float)angle);
				g.DrawImage(bmp, 0, 0);
			}
			finally
			{
				g.Dispose();
			}
			return tmp;
		}
	}

	class ImageHelper
	{
		/// <summary>
		/// Rescales an image.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="dpiX"></param>
		/// <param name="dpiY"></param>
		/// <returns></returns>
		public static Image Rescale(Image image, int dpiX, int dpiY)
		{
			Bitmap bm = new Bitmap((int)(image.Width * dpiX / image.HorizontalResolution), (int)(image.Height * dpiY / image.VerticalResolution));
			bm.SetResolution(dpiX, dpiY);
			Graphics g = Graphics.FromImage(bm);
			g.InterpolationMode = InterpolationMode.Bicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.DrawImage(image, 0, 0);
			g.Dispose();

			return bm;
		}

		/// <summary>
		/// Crops an image.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="cropArea"></param>
		/// <returns></returns>
		public static Image Crop(Image image, Rectangle cropArea)
		{
			Bitmap bmp = new Bitmap(cropArea.Width, cropArea.Height);
			bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			Graphics gfx = Graphics.FromImage(bmp);
			gfx.DrawImage(image, 0, 0, cropArea, GraphicsUnit.Pixel);
			gfx.Dispose();

			return bmp;
		}

		/// <summary>
		/// Crops an image (another method).
		/// </summary>
		/// <param name="image"></param>
		/// <param name="cropArea"></param>
		/// <returns></returns>
		//public static Image Crop(Image image, Rectangle cropArea)
		//{
		//    Bitmap bitmap = new Bitmap(image);
		//    bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
		//    return bitmap.Clone(cropArea, image.PixelFormat); // this has thrown OutOfMemoryException on WinXP
		//}

		/// <summary>
		/// Rotates an image.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static Bitmap Rotate(Image image, double angle)
		{
			Bitmap bm = new Bitmap(image.Width, image.Height);
			bm.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			Graphics g = Graphics.FromImage(bm);
			//move rotation point to center of image
			g.TranslateTransform((float)image.Width / 2, (float)image.Height / 2);
			//rotate
			g.RotateTransform((float)angle);
			//move image back
			g.TranslateTransform(-(float)image.Width / 2, -(float)image.Height / 2);
			//draw passed in image onto graphics object
			g.DrawImage(image, 0, 0);
			g.Dispose();

			return bm;
		}
	}

	public class TextExtractorOcr : Processor<TextExtractorOcrData>
	{
		private readonly OCR.TesseractWrapper.TesseractProcessor tessocr = new OCR.TesseractWrapper.TesseractProcessor();
		readonly string basedir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		const string TESSDATA = "tessdata/";
		const double MINIMUM_DESKEW_THRESHOLD = 0.05d;

		public TextExtractorOcr()
		{
			string tessdata = System.IO.Path.Combine(basedir, TESSDATA);
			tessocr.Init(tessdata, "eng", (int)OCR.TesseractWrapper.ePageSegMode.PSM_AUTO);
		}

		public override void Process(TextExtractorOcrData data)
		{
			WFLogger.NLogger.Info("DocumentToProcess={0}", data.DocumentToProcess);

			tessocr.Clear();

			data.WFState.Value = WFState.WFStateFail;

			System.IO.StreamWriter file = null;
			try
			{
				IList<Image> images = GetImageList(data.DocumentToProcess);

				if (images != null && images.Count > 0)
					file = new System.IO.StreamWriter(string.Format(@"{0}.ocr", data.DocumentToProcess));

				foreach (System.Drawing.Image image in GetImageList(data.DocumentToProcess))
				{
					System.Drawing.Image deskewedimage = image;

					// deskew the image
					gmseDeskew deskew = new gmseDeskew((System.Drawing.Bitmap)image);
					double imageSkewAngle = deskew.GetSkewAngle();

					if ((imageSkewAngle > MINIMUM_DESKEW_THRESHOLD || imageSkewAngle < -(MINIMUM_DESKEW_THRESHOLD)))
						deskewedimage = ImageHelper.Rotate(image, imageSkewAngle);

					try
					{
//						string tessdata = System.IO.Path.Combine(basedir, TESSDATA);

						StringBuilder sb = new StringBuilder();
//						if (true == tessocr.Init(tessdata, "eng", (int)OCR.TesseractWrapper.ePageSegMode.PSM_AUTO))
						{
							file.WriteLine(tessocr.Recognize(deskewedimage));
#if false
							string prevword = string.Empty;
							OCR.TesseractWrapper.DocumentLayout dl = tessocr.AnalyseLayout((System.Drawing.Bitmap)image);
							foreach (OCR.TesseractWrapper.Block block in dl.Blocks)
							{
								foreach (OCR.TesseractWrapper.Paragraph paragraph in block.Paragraphs)
								{
									foreach (OCR.TesseractWrapper.TextLine line in paragraph.Lines)
									{
										foreach (OCR.TesseractWrapper.Word word in line.Words)
										{
											System.Drawing.Rectangle part = new Rectangle(word.Left, word.Top, word.Right - word.Left + 1, word.Bottom - word.Top + 1);
											tessocr.UseROI = true;
											tessocr.ROI = part;
											string recognizedword = tessocr.Recognize(image).Replace("\n", "");
											recognizedword = System.Text.RegularExpressions.Regex.Replace(recognizedword, @"[^\w\.@-]", string.Empty);
											prevword = recognizedword;
										}
									}
									sb.Append("\n");
								}
								sb.Append("\n");
							}
#endif
						}
					}
					catch (Exception ex)
					{
						WFLogger.NLogger.ErrorException("ERROR: Could not instantiate TesseractProcessor.", ex);
					}
					image.Dispose();
				}
			}
			catch(Exception ex)
			{
				WFLogger.NLogger.ErrorException("ERROR: GetImageList failed.", ex);
			}
			finally
			{
				if (file != null)
				{
					file.Flush();
					file.Close();
					file.Dispose();
				}
//					image.Dispose();
			}

			data.WFState.Value = WFState.WFStateSuccess;
		}

		private string ConvertPdf2Tiff(string inputPdfFile)
		{
			string[] pngFiles = null;

			try
			{
				pngFiles = ConvertPdf2Png(inputPdfFile);
				string tiffFile = System.IO.Path.GetTempFileName();
				System.IO.File.Delete(tiffFile);
				tiffFile = System.IO.Path.ChangeExtension(tiffFile, ".tif");

				// put PNG images into a single multi-page TIFF image for return
				MergeTiff(pngFiles, tiffFile);
				return tiffFile;
			}
			catch (ApplicationException ae)
			{
				WFLogger.NLogger.ErrorException("ERROR: Converting pdf to tiff failed.", ae);
				throw;
			}
			finally
			{
				if (pngFiles != null)
				{
					// delete temporary PNG images
					foreach (string tempFile in pngFiles)
					{
						System.IO.File.Delete(tempFile);
					}
				}
			}
		}

		private string[] ConvertPdf2Png(string inputPdfFile)
		{
			PDFConvert converter = new PDFConvert();
			converter.GraphicsAlphaBit = 4;
			converter.TextAlphaBit = 4;
			converter.ResolutionX = 300; // -r300
			converter.OutputFormat = "pnggray"; // -sDEVICE
			converter.ThrowOnlyException = true; // rethrow exceptions

			string sOutputFile = string.Format("{0}\\workingimage%03d.png", System.IO.Path.GetDirectoryName(inputPdfFile));
			bool success = converter.Convert(inputPdfFile, sOutputFile);

			if (success)
			{
				// find working files
				string[] workingFiles = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(inputPdfFile), "workingimage???.png");
				Array.Sort(workingFiles);
				return workingFiles;
			}
			else
			{
				return new string[0];
			}
		}

		private void MergeTiff(string[] inputImages, string outputTiff)
		{
			//get the codec for tiff files
			ImageCodecInfo info = null;

			foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
			{
				if (ice.MimeType == "image/tiff")
				{
					info = ice;
				}
			}

			//use the save encoder
			System.Drawing.Imaging.Encoder enc = System.Drawing.Imaging.Encoder.SaveFlag;
			EncoderParameters ep = new EncoderParameters(2);
			ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);
			System.Drawing.Imaging.Encoder enc1 = System.Drawing.Imaging.Encoder.Compression;
			ep.Param[1] = new EncoderParameter(enc1, (long)EncoderValue.CompressionNone);
			Bitmap pages = null;

			try
			{
				int frame = 0;

				foreach (string inputImage in inputImages)
				{
					if (frame == 0)
					{
						pages = (Bitmap)Image.FromFile(inputImage);
						//save the first frame
						pages.Save(outputTiff, info, ep);
					}
					else
					{
						//save the intermediate frames
						ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);
						Bitmap bm = null;
						try
						{
							bm = (Bitmap)Image.FromFile(inputImage);
							pages.SaveAdd(bm, ep);
						}
						catch (System.Runtime.InteropServices.ExternalException e)
						{
							throw new ApplicationException(e.Message + "\nIt might have run out of memory due to handling too many images or too large a file.", e);
						}
						finally
						{
							if (bm != null)
							{
								bm.Dispose();
							}
						}
					}

					if (frame == inputImages.Length - 1)
					{
						//flush and close
						ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.Flush);
						pages.SaveAdd(ep);
					}
					frame++;
				}
			}
			finally
			{
				if (pages != null)
				{
					pages.Dispose();
				}
			}
		}

		private IList<Image> GetImageList(string imageFile)
		{
			string workingTiffFileName = null;

			Image image = null;

			try
			{
				// convert PDF to TIFF
				if (KRSrcWorkflow.WFFileType.GetFileType(imageFile) == KRSrcWorkflow.WFFileType.FileType.Pdf)
				{
					workingTiffFileName = ConvertPdf2Tiff(imageFile);
					imageFile = workingTiffFileName;
				}

				// read in the image
				image = Image.FromFile(imageFile);

				IList<Image> images = new List<Image>();

				int count;
				if (image.RawFormat.Equals(ImageFormat.Gif))
				{
					count = image.GetFrameCount(FrameDimension.Time);
				}
				else
				{
					count = image.GetFrameCount(FrameDimension.Page);
				}

				for (int i = 0; i < count; i++)
				{
					// save each frame to a bytestream
					using (System.IO.MemoryStream byteStream = new System.IO.MemoryStream())
					{
						image.SelectActiveFrame(FrameDimension.Page, i);
						image.Save(byteStream, ImageFormat.Png);

						// and then create a new Image from it
						images.Add(Image.FromStream(byteStream));
					}
				}

				return images;
			}
			catch (OutOfMemoryException e)
			{
				throw new ApplicationException(e.Message, e);
			}
			catch (System.Runtime.InteropServices.ExternalException e)
			{
				throw new ApplicationException(e.Message + "\nIt might have run out of memory due to handling too many images or too large a file.", e);
			}
			finally
			{
				if (image != null)
				{
					image.Dispose();
				}

				if (workingTiffFileName != null && System.IO.File.Exists(workingTiffFileName))
				{
					System.IO.File.Delete(workingTiffFileName);
				}
			}
		}
	}
}
