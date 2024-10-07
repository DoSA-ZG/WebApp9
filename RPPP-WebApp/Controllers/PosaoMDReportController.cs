using RPPP_WebApp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using RPPP_WebApp.Extensions;
using System.Security.Permissions;
using RPPP_WebApp.ViewModels;
using Microsoft.IdentityModel.Tokens;

namespace MVC.Controllers
{
    public class PosaoMDReportController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vpd.openxmlformats-officedocument.spreadsheetml.sheet";

        public PosaoMDReportController(RPPP09Context ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Posao()
        {
            string naslov = "Popis poslova";
            var posloviF = await ctx.Posao
                                      .Include(p => p.Projekt)
                                      .Include(p => p.VrstaPosla)
                                      .AsNoTracking()
                                      .OrderBy(p => p)
                                      .ToListAsync();

            var poslovi = new List<PosaoReportModel>();

            foreach (var posao in posloviF)
                poslovi.Add(PosaoReportModel.FromPosao(posao));

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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(poslovi));

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
                    column.PropertyName<PosaoReportModel>(p => p.Projekt.ImeProjekta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Projekt", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoReportModel>(p => p.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoReportModel>(p => p.OcekivaniPocetak);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Ocekivani pocetak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoReportModel>(p => p.OcekivaniZavrsetak);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Ocekivani zavrsetak", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoReportModel>(p => p.Budzet);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Budzet", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoReportModel>(p => p.VrstaPosla.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Vrsta posla", horizontalAlignment: HorizontalAlignment.Center);
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

        public async Task<IActionResult> VrstaPosla()
        {
            string naslov = "Popis vrsta poslova";
            var vrstaPosla = await ctx.VrstaPosla
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
                    column.PropertyName<VrstaPosla>(vp => vp.Naziv);
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

        public async Task<IActionResult> Suradnik()
        {
            string naslov = "Popis suradnika";
            var prolasciVozila = await ctx.Suradnik
                                    .AsNoTracking()
                                    .Include(s => s.VrstaSuradnika)
                                    .Include(s => s.Posao)
                                    .OrderBy(d => d.Id)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(prolasciVozila));

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
                    column.PropertyName<Suradnik>(s => s.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(s => s.Oib);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Oib", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(s => s.Adresa);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Adresa", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(s => s.PostanskiBroj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Postanski broj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(s => s.Grad);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Grad", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(s => s.VrstaSuradnika.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Vrsta suradnika", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Suradnik>(s => s.Posao.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Posao", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=odrzavanjeObjekata.pdf");
                return File(pdf, "application/pdf");
                //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> VrstaSuradnika()
        {
            string naslov = "Vrste suradnika";
            var kategorije = await ctx.VrstaSuradnika
                                    .AsNoTracking()
                                    .OrderBy(vs => vs.Naziv)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(kategorije));

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
                    column.PropertyName<VrstaSuradnika>(vs => vs.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<VrstaSuradnika>(vs => vs.Opis);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Opis", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=vrsteOdrzavanja.pdf");
                return File(pdf, "application/pdf");
                //return File(pdf, "application/pdf", "drzave.pdf"); //Otvara save as dialog
            }
            else
            {
                return NotFound();
            }
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
                fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                           Path.Combine(environment.WebRootPath, "fonts", "tahoma.ttf"));
                fonts.Size(7);
                fonts.Color(System.Drawing.Color.Black);
            })
            //
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

        public async Task<IActionResult> MDPoslovi()
        {
            string naslov = "Josip MD poslovi";
            var poslovi = await ctx.Posao.Include(p => p.VrstaPosla).Include(p => p.Projekt)
                                    .Include(p => p.Suradnik).ToListAsync();
            List<PosaoSuradnik> posaoMD = new List<PosaoSuradnik>();
            foreach (var p in poslovi)
            {
                List<Suradnik> suradnici = await ctx.Suradnik.Where(s => s.PosaoId == p.Id).Include(s => s.VrstaSuradnika).ToListAsync();
                Boolean zeroAdded = suradnici.IsNullOrEmpty();

                foreach(var suradnik in suradnici)
                {
                    var dto = new PosaoSuradnik();
                    dto.Posao = p;
                    dto.Suradnik = suradnik;
                    posaoMD.Add(dto);
                }

                if (zeroAdded)
                {
                    PosaoSuradnik newMD = new PosaoSuradnik();
                    newMD.Posao = p;
                    Suradnik s = new Suradnik();
                    s.VrstaSuradnika = new VrstaSuradnika();
                    newMD.Suradnik = s;
                    posaoMD.Add(newMD);
                }

                
            }
            posaoMD.OrderBy(km => km.Posao.Id).OrderBy(km => km.Suradnik.Naziv);
            PdfReport report = CreateReport(naslov);
            #region Podnožje i zaglavlje
            report.PagesFooter(footer =>
            {
                footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
            })
            .PagesHeader(header =>
            {
                header.CacheHeader(cache: true); // It's a default setting to improve the performance.
                header.CustomHeader(new MasterDetailsHeaders(naslov)
                {
                    PdfRptFont = header.PdfFont
                });
            });
            #endregion
            #region Postavljanje izvora podataka i stupaca
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(posaoMD));

            report.MainTableColumns(columns =>
            {
                #region Stupci po kojima se grupira
                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoMDViewModel>(km => km.Posao.Id);
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
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoSuradnik>(s => s.Suradnik.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoSuradnik>(s => s.Suradnik.Oib);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Oib", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoSuradnik>(s => s.Suradnik.Adresa);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Adresa", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoSuradnik>(s => s.Suradnik.PostanskiBroj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Postanski broj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoSuradnik>(s => s.Suradnik.Grad);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Grad", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PosaoSuradnik>(s => s.Suradnik.VrstaSuradnika.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Vrsta suradnika", horizontalAlignment: HorizontalAlignment.Center);
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

        #region Export u Excel
        public async Task<IActionResult> ExcelSimplePosao()
        {
            var posao = await ctx.Posao
                                  .Include(p => p.Projekt)
                                  .Include(p => p.VrstaPosla)
                                  .AsNoTracking()
                                  .OrderBy(p => p.Id)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis poslova";
                excel.Workbook.Properties.Author = "Josip poslovi";
                var worksheet = excel.Workbook.Worksheets.Add("Poslovi");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Projekt";
                worksheet.Cells[1, 3].Value = "Naziv";
                worksheet.Cells[1, 4].Value = "Ocekivani pocetak";
                worksheet.Cells[1, 5].Value = "Ocekivani zavrsetak";
                worksheet.Cells[1, 6].Value = "Budzet(EUR)";
                worksheet.Cells[1, 7].Value = "Vrsta posla";

                for (int i = 0; i < posao.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = posao[i].Id;
                    worksheet.Cells[i + 2, 2].Value = posao[i].Projekt.ImeProjekta;
                    worksheet.Cells[i + 2, 3].Value = posao[i].Naziv;
                    worksheet.Cells[i + 2, 4].Value = posao[i].OcekivaniPocetak.ToString("dd.MM.yyyy.");
                    worksheet.Cells[i + 2, 5].Value = posao[i].OcekivaniZavrsetak.ToString("dd.MM.yyyy.");
                    worksheet.Cells[i + 2, 6].Value = posao[i].Budzet;
                    worksheet.Cells[i + 2, 7].Value = posao[i].VrstaPosla.Naziv;
                }

                worksheet.Cells[1, 1, posao.Count + 1, 11].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "poslovi.xlsx");
        }

        public async Task<IActionResult> ExcelSimpleVrstePoslova()
        {
            var vrstePoslova = await ctx.VrstaPosla
                                    .AsNoTracking()
                                    .OrderBy(vp => vp.IdVrstaPosla)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis vrsta poslova";
                excel.Workbook.Properties.Author = "Josip vrste poslova";
                var worksheet = excel.Workbook.Worksheets.Add("Vrste poslova");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv";

                for (int i = 0; i < vrstePoslova.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = vrstePoslova[i].IdVrstaPosla;
                    worksheet.Cells[i + 2, 2].Value = vrstePoslova[i].Naziv;
                }

                worksheet.Cells[1, 1, vrstePoslova.Count + 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "vrstePoslova.xlsx");
        }

        public async Task<IActionResult> ExcelSimpleSuradnik()
        {
            var suradnici = await ctx.Suradnik
                                    .Include(s => s.VrstaSuradnika)
                                    .AsNoTracking()
                                    .OrderBy(s => s.Id)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis suradnika";
                excel.Workbook.Properties.Author = "Josip";
                var worksheet = excel.Workbook.Worksheets.Add("Suradnici");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv";
                worksheet.Cells[1, 3].Value = "Oib";
                worksheet.Cells[1, 4].Value = "Adresa";
                worksheet.Cells[1, 5].Value = "Postanski broj";
                worksheet.Cells[1, 6].Value = "Grad";
                worksheet.Cells[1, 7].Value = "Vrsta suradnika";

                for (int i = 0; i < suradnici.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = suradnici[i].Id;
                    worksheet.Cells[i + 2, 2].Value = suradnici[i].Naziv;
                    worksheet.Cells[i + 2, 3].Value = suradnici[i].Oib;
                    worksheet.Cells[i + 2, 4].Value = suradnici[i].Adresa;
                    worksheet.Cells[i + 2, 5].Value = suradnici[i].PostanskiBroj;
                    worksheet.Cells[i + 2, 6].Value = suradnici[i].Grad;
                    worksheet.Cells[i + 2, 7].Value = suradnici[i].VrstaSuradnika.Naziv;

                    worksheet.Cells[1, 1, suradnici.Count + 1, 9].AutoFitColumns();

                }
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "suradnici.xlsx");
        }

        public async Task<IActionResult> ExcelSimpleVrstaSuradnika()
        {
            var vrsteSuradnika = await ctx.VrstaSuradnika
                                    .AsNoTracking()
                                    .OrderBy(vs => vs.IdVrstaSuradnika)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis vrsta suradnika";
                excel.Workbook.Properties.Author = "Josip";
                var worksheet = excel.Workbook.Worksheets.Add("Vrste suradnika");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv";
                worksheet.Cells[1, 3].Value = "Opis";

                for (int i = 0; i < vrsteSuradnika.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = vrsteSuradnika[i].IdVrstaSuradnika;
                    worksheet.Cells[i + 2, 2].Value = vrsteSuradnika[i].Naziv;
                    worksheet.Cells[i + 2, 3].Value = vrsteSuradnika[i].Opis;
                }

                worksheet.Cells[1, 1, vrsteSuradnika.Count + 1, 5].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "vrsteSuradnika.xlsx");
        }

        public async Task<IActionResult> ExcelComplexPosao()
        {
            var posao = await ctx.Posao.Include(p => p.VrstaPosla).Include(p => p.Projekt).
                OrderBy(p => p.Id).ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "MD poslovi";
                excel.Workbook.Properties.Author = "Josip";
                var worksheet = excel.Workbook.Worksheets.Add("MD poslovi");
                worksheet.Cells[2, 1].Value = "Posao";
                worksheet.Cells[4, 1].Value = "Suradnici";
                worksheet.Cells[2, 1].AutoFitColumns();
                worksheet.Cells[4, 1].AutoFitColumns();


                //First add the headers
                for (int i = 0; i < posao.Count; i++)
                {
                    worksheet.Cells[1, i * 11 + 2].Value = "Id";
                    worksheet.Cells[1, i * 11 + 3].Value = "Projekt";
                    worksheet.Cells[1, i * 11 + 4].Value = "Naziv";
                    worksheet.Cells[1, i * 11 + 5].Value = "Ocekivani pocetak";
                    worksheet.Cells[1, i * 11 + 6].Value = "Ocekivani zavrsetak";
                    worksheet.Cells[1, i * 11 + 7].Value = "Budzet(EUR)";
                    worksheet.Cells[1, i * 11 + 8].Value = "Vrsta posla";
                    worksheet.Cells[1, i * 11 + 9].Value = "|";
                    worksheet.Cells[2, i * 11 + 2].Value = posao[i].Id;
                    worksheet.Cells[2, i * 11 + 3].Value = posao[i].Projekt.ImeProjekta;
                    worksheet.Cells[2, i * 11 + 4].Value = posao[i].Naziv;
                    worksheet.Cells[2, i * 11 + 5].Value = posao[i].OcekivaniPocetak.ToString("dd.MM.yyyy.");
                    worksheet.Cells[2, i * 11 + 6].Value = posao[i].OcekivaniZavrsetak.ToString("dd.MM.yyyy.");
                    worksheet.Cells[2, i * 11 + 7].Value = posao[i].Budzet;
                    worksheet.Cells[2, i * 11 + 8].Value = posao[i].VrstaPosla;
                    worksheet.Cells[2, i * 11 + 9].Value = "|";

                    var suradnici = await ctx.Suradnik.Where(s => s.PosaoId == posao[i].Id).Include(s => s.VrstaSuradnika).ToListAsync();
                    worksheet.Cells[4, i * 11 + 2].Value = "Id";
                    worksheet.Cells[4, i * 11 + 3].Value = "Naziv";
                    worksheet.Cells[4, i * 11 + 4].Value = "Oib";
                    worksheet.Cells[4, i * 11 + 5].Value = "Adresa";
                    worksheet.Cells[4, i * 11 + 6].Value = "Postanski broj";
                    worksheet.Cells[4, i * 11 + 7].Value = "Grad";
                    worksheet.Cells[4, i * 11 + 8].Value = "Vrsta suradnika";
                    worksheet.Cells[4, i * 11 + 2].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 3].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 4].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 5].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 6].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 7].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 8].AutoFitColumns();
                    for (int j = 0; j < suradnici.Count; j++)
                    {
                        worksheet.Cells[j + 5, i * 11 + 2].Value = suradnici[j].Id;
                        worksheet.Cells[j + 5, i * 11 + 3].Value = suradnici[j].Naziv;
                        worksheet.Cells[j + 5, i * 11 + 4].Value = suradnici[j].Oib;
                        worksheet.Cells[j + 5, i * 11 + 5].Value = suradnici[j].Adresa;
                        worksheet.Cells[j + 5, i * 11 + 6].Value = suradnici[j].PostanskiBroj;
                        worksheet.Cells[j + 5, i * 11 + 7].Value = suradnici[j].Grad;
                        worksheet.Cells[j + 5, i * 11 + 8].Value = suradnici[j].VrstaSuradnika.Naziv;
                        worksheet.Cells[j + 5, i * 11 + 2].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 3].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 5].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 6].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 7].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 8].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 4].AutoFitColumns();
                    }
                }

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Poslovi.xlsx");
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
                var posao = (Posao)newGroupInfo.GetValueOf(nameof(PosaoSuradnik.Posao));


                var table = new PdfGrid(relativeWidths: new[] { 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f }) { WidthPercentage = 100 };

                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Id:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = posao.Id;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Projekt:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = posao.Projekt.ImeProjekta;
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
                        cellData.Value = posao.Naziv;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Ocekivani pocetak";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = posao.OcekivaniPocetak.ToString("dd.MM.yyyy.");
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                (cellData, cellProperties) => {
                    cellData.Value = "Ocekivani zavrsetak";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
            },
                (cellData, cellProperties) =>
                {
                    cellData.Value = posao.OcekivaniZavrsetak.ToString("dd.MM.yyyy.");
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) => {
                    cellData.Value = "Budzet(EUR)";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = posao.Budzet;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) => {
                    cellData.Value = "Vrsta posla";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = posao.VrstaPosla.Naziv;
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
