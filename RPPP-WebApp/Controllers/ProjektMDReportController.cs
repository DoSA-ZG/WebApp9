using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System;
using OfficeOpenXml;

namespace RPPP_WebApp.Controllers
{
    public class ProjektMDReportController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly IWebHostEnvironment _environment;
        private const string ExcelContentType = "application/vpd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ProjektMDReportController(RPPP09Context ctx, IWebHostEnvironment environment)
        {
            this._context = ctx;
            this._environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Projekt()
        {
            string naslov = "Popis projekata";
            var projekti = await _context.Projekt
                                      .Include(p => p.VrstaProjekta)
                                      .Include(p => p.Posao)
                                      .Include(p => p.Dokument)
                                      .Include(p => p.Racun)
                                      .AsNoTracking()
                                      .OrderBy(p => p)
                                      .ToListAsync();

            PdfReport report = CreateReport(naslov);
            #region Podnožje i zaglavlje
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); 
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });
            #endregion
            #region Postavljanje izvora podataka i stupaca
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(projekti));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Projekt>(p => p.ImeProjekta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Projekt>(p => p.PocetakProjekta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Početak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Projekt>(p => p.KrajProjekta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Završetak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Projekt>(p => p.Oznaka);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Oznaka", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Projekt>(p => p.VrstaProjekta.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Vrsta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Projekt>(p => p.Racun.Iban);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Račun", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Projekt>(p => p.Opis);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=naplatneKucice.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> VrstaProjekta()
        {
            string naslov = "Popis vrsta projekata";
            var vrstaPosla = await _context.VrstaProjekta
                                    .AsNoTracking()
                                    .OrderBy(vp => vp.Naziv)
                                    .ToListAsync();
            PdfReport report = CreateReport(naslov);
            #region Podnožje i zaglavlje
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });
            #endregion
            #region Postavljanje izvora podataka i stupaca
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(vrstaPosla));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<VrstaProjekta>(vp => vp.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=VrstaPosla.pdf");
                return File(pdf, "application/pdf");
                //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Dokument()
        {
            string naslov = "Popis dokumenata";
            var projekti = await _context.Dokument
                                      .Include(p => p.KategorijaDokumenta)
                                      .Include(p => p.Projekt)
                                      .AsNoTracking()
                                      .OrderBy(p => p)
                                      .ToListAsync();

            PdfReport report = CreateReport(naslov);
            #region Podnožje i zaglavlje
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true);
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });
            #endregion
            #region Postavljanje izvora podataka i stupaca
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(projekti));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Dokument>(p => p.Naslov);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naslov", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Dokument>(p => p.Projekt.ImeProjekta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Naziv projekta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Dokument>(p => p.KategorijaDokumenta.NazivKategorijeDokumenta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Kategorija", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Dokument>(p => p.Sadrzaj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(5);
                    column.HeaderCell("Sadržaj", horizontalAlignment: HorizontalAlignment.Center);
                });

            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=naplatneKucice.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> KategorijaDokumenta()
        {
            string naslov = "Popis kategorija dokumenata";
            var vrstaPosla = await _context.KategorijaDokumenta
                                    .AsNoTracking()
                                    .OrderBy(vp => vp.NazivKategorijeDokumenta)
                                    .ToListAsync();
            PdfReport report = CreateReport(naslov);
            #region Podnožje i zaglavlje
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                header.DefaultHeader(defaultHeader =>
                {
                    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    defaultHeader.Message(naslov);
                });
            });
            #endregion
            #region Postavljanje izvora podataka i stupaca
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(vrstaPosla));

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<KategorijaDokumenta>(vp => vp.NazivKategorijeDokumenta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=VrstaPosla.pdf");
                return File(pdf, "application/pdf");
                //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> MDProjekti()
        {
            string naslov = "MD projekti";
            var projekti = await _context.Projekt
                                        .Include(p => p.VrstaProjekta)
                                        .Include(p => p.Posao)
                                        .Include(p => p.Dokument)
                                            .ThenInclude(d => d.KategorijaDokumenta)
                                        .Include(p => p.Racun)
                                        .OrderBy(p => p)
                                        .ToListAsync();
            List<ProjektDokument> projektDokument = new();
            foreach(var p in projekti)
            {
                foreach(var d in p.Dokument)
                {
                    var pd = new ProjektDokument
                    {
                        Projekt = p,
                        Dokument = d
                    };
                    projektDokument.Add(pd);
                }

                if(p.Dokument.IsNullOrEmpty())
                {
                    var pd = new ProjektDokument
                    {
                        Projekt = p,
                        Dokument = new Dokument()
                    };
                    pd.Dokument.KategorijaDokumenta = new KategorijaDokumenta();
                    projektDokument.Add(pd);
                }
            }

            projektDokument  = projektDokument.OrderBy(p => p.Projekt.IdProjekt).ThenBy(d => d.Dokument.Naslov).ToList();
            PdfReport report = CreateReport(naslov);
            #region Podnožje i zaglavlje
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); 
                header.CustomHeader(new MasterDetailsHeaders(naslov)
                {
                    PdfRptFont = header.PdfFont
                });
            });
            #endregion
            #region Postavljanje izvora podataka i stupaca
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(projektDokument));

            report.MainTableColumns(columns =>
            {
                #region Stupci po kojima se grupira
                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektMDViewModel>(p => p.Projekt.IdProjekt);
                    column.Group(
                        (val1, val2) =>
                        {
                            return (int)val1 == (int)val2;
                        });
                });
                #endregion
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Order(0);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektDokument>(p => p.Dokument.Naslov);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naslov", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektDokument>(p => p.Dokument.KategorijaDokumenta.NazivKategorijeDokumenta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Kategorija", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ProjektDokument>(p => p.Dokument.Sadrzaj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(5);
                    column.HeaderCell("Sadržaj", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

          

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=MDCestovpiObjekti.pdf");
                return File(pdf, "application/pdf");
            }
            else
                return NotFound();
        }

        private PdfReport CreateReport(string naslov)
        {
            var pdf = new PdfReport();

            pdf.DocumentPreferences(doc =>
            {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "RPPP09",
                    Application = "RPPP_WebApp.MVC Core",
                    Title = naslov
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
            .DefaultFonts(fonts =>
            {
                fonts.Path(Path.Combine(_environment.WebRootPath, "fonts", "verdana.ttf"),
                           Path.Combine(_environment.WebRootPath, "fonts", "tahoma.ttf"));
                fonts.Size(7);
                fonts.Color(System.Drawing.Color.Black);
            })
            .MainTableTemplate(template =>
            {
                template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
            })
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                table.GroupsPreferences(new GroupsPreferences
                {
                    GroupType = GroupType.HideGroupingColumns,
                    RepeatHeaderRowPerGroup = true,
                    ShowOneGroupPerPage = true,
                    SpacingBeforeAllGroupsSummary = 5f,
                    NewGroupAvailableSpacingThreshold = 150,
                    SpacingAfterAllGroupsSummary = 5f
                });
                table.SpacingAfter(4f);
            });

            return pdf;
        }

        #region Export u Excel
        public async Task<IActionResult> ExcelProjekt()
        {
            var projekti = await _context.Projekt
                                      .Include(p => p.VrstaProjekta)
                                      .Include(p => p.Posao)
                                      .Include(p => p.Dokument)
                                      .Include(p => p.Racun)
                                      .AsNoTracking()
                                      .OrderBy(p => p)
                                      .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis projekata";
                var worksheet = excel.Workbook.Worksheets.Add("Projekti");

                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv";
                worksheet.Cells[1, 3].Value = "Početak";
                worksheet.Cells[1, 4].Value = "Završetak";
                worksheet.Cells[1, 5].Value = "Oznaka";
                worksheet.Cells[1, 6].Value = "Vrsta";
                worksheet.Cells[1, 7].Value = "Račun";
                worksheet.Cells[1, 8].Value = "Opis";



                for (int i = 0; i < projekti.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = projekti[i].IdProjekt;
                    worksheet.Cells[i + 2, 2].Value = projekti[i].ImeProjekta;
                    worksheet.Cells[i + 2, 3].Value = projekti[i].PocetakProjekta.ToString("dd.MM.yyyy.");
                    worksheet.Cells[i + 2, 4].Value = projekti[i].KrajProjekta.ToString("dd.MM.yyyy.");
                    worksheet.Cells[i + 2, 5].Value = projekti[i].Oznaka;
                    worksheet.Cells[i + 2, 6].Value = projekti[i].VrstaProjekta.Naziv;
                    worksheet.Cells[i + 2, 7].Value = projekti[i].Racun.Iban;
                    worksheet.Cells[i + 2, 8].Value = projekti[i].Opis;
                }

                worksheet.Cells[1, 1, projekti.Count + 1, 11].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "projekti.xlsx");
        }

        public async Task<IActionResult> ExcelVrstaProjekta()
        {
            var vrsteProjeakata = await _context.VrstaProjekta
                                    .AsNoTracking()
                                    .OrderBy(vp => vp.IdVrstaProjekta)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis vrsta projekata";
                var worksheet = excel.Workbook.Worksheets.Add("Vrste projekata");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv";

                for (int i = 0; i < vrsteProjeakata.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = vrsteProjeakata[i].IdVrstaProjekta;
                    worksheet.Cells[i + 2, 2].Value = vrsteProjeakata[i].Naziv;
                }

                worksheet.Cells[1, 1, vrsteProjeakata.Count + 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "vrsteProjekata.xlsx");
        }

        public async Task<IActionResult> ExcelDokument()
        {
            var dokumenti = await _context.Dokument
                                    .Include(d => d.KategorijaDokumenta)
                                    .Include(p => p.Projekt)
                                    .AsNoTracking()
                                    .OrderBy(d => d.DokumentId)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis dokumenata";
                var worksheet = excel.Workbook.Worksheets.Add("Dokumenti");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Projekt";
                worksheet.Cells[1, 3].Value = "Naslov";
                worksheet.Cells[1, 4].Value = "Kategorija";
                worksheet.Cells[1, 5].Value = "Sadržaj";

                for (int i = 0; i < dokumenti.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = dokumenti[i].DokumentId;
                    worksheet.Cells[i + 2, 2].Value = dokumenti[i].Projekt.ImeProjekta;
                    worksheet.Cells[i + 2, 3].Value = dokumenti[i].Naslov;
                    worksheet.Cells[i + 2, 4].Value = dokumenti[i].KategorijaDokumenta.NazivKategorijeDokumenta;
                    worksheet.Cells[i + 2, 5].Value = dokumenti[i].Sadrzaj;

                    worksheet.Cells[1, 1, dokumenti.Count + 1, 9].AutoFitColumns();

                }
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "dokumenti.xlsx");
        }

        public async Task<IActionResult> ExcelKategorijaDokumenta()
        {
            var kategorije = await _context.KategorijaDokumenta
                                    .AsNoTracking()
                                    .OrderBy(kd => kd.KategorijaDokumentaId)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis kategorija dokumenata";
                var worksheet = excel.Workbook.Worksheets.Add("Vrste suradnika");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv";

                for (int i = 0; i < kategorije.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = kategorije[i].KategorijaDokumentaId;
                    worksheet.Cells[i + 2, 2].Value = kategorije[i].NazivKategorijeDokumenta;
                }

                worksheet.Cells[1, 1, kategorije.Count + 1, 5].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "kategorijeDokumenata.xlsx");
        }


        #endregion

        #region Master-detail header
        public class MasterDetailsHeaders : IPageHeader
        {
            private string naslov;
            public MasterDetailsHeaders(string naslov)
            {
                this.naslov = naslov;
            }
            public IPdfFont PdfRptFont { set; get; }

            public PdfGrid RenderingGroupHeader(Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo, IList<SummaryCellData> summaryData)
            {
                var projekt = (Projekt)newGroupInfo.GetValueOf(nameof(ProjektDokument.Projekt));


                var table = new PdfGrid(relativeWidths: new[] { 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f }) { WidthPercentage = 100 };

                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Id";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = projekt.IdProjekt;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Naziv";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = projekt.ImeProjekta;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Početak";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = projekt.PocetakProjekta.ToString("dd.MM.yyyy.");
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                (cellData, cellProperties) => {
                    cellData.Value = "Završetak";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = projekt.KrajProjekta.ToString("dd.MM.yyyy.");
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) => {
                    cellData.Value = "Oznaka";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = projekt.Oznaka;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) => {
                    cellData.Value = "Vrsta";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = projekt.VrstaProjekta.Naziv;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) => {
                    cellData.Value = "Račun";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = projekt.Racun.ImeRacuna;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                });
                return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
            }

            public PdfGrid RenderingReportHeader(Document pdfDoc, PdfWriter pdfWriter, IList<SummaryCellData> summaryData)
            {
                var table = new PdfGrid(numColumns: 1) { WidthPercentage = 100 };
                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = naslov;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Center;
                    });
                return table.AddBorderToTable();
            }
        }
    }
    #endregion
}
  

