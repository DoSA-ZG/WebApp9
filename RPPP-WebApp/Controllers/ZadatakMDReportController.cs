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
using static MVC.Controllers.PosaoMDReportController;

namespace MVC.Controllers
{
    /// <summary>
    /// Kontroler za generiranje izvještaja za zadatke.
    /// </summary>
    public class ZadatakMDReportController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vpd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// Konstruktor za ZadatakMDReportController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="environment">Okruženje aplikacije.</param>
        public ZadatakMDReportController(RPPP09Context ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }

        /// <summary>
        /// Prikazuje početnu stranicu za generiranje izvještaja za zadatke.
        /// </summary>
        /// <returns>Rezultat akcije koji predstavlja prikaz početne stranice.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Akcija za generiranje PDF izvještaja o zadacima.
        /// </summary>
        /// <returns>Rezultat akcije koji predstavlja PDF izvještaj o zadacima.</returns>
        public async Task<IActionResult> Zadatak()
        {
            string naslov = "Popis zadataka";
            var zadaciF = await ctx.Zadatak
                .Include(p => p.Zahtjev)
                .Include(p => p.VrstaZadatka)
                .AsNoTracking()
                .OrderBy(p => p)
                .ToListAsync();

            var zadaci = new List<ZadatakReportModel>();

            foreach (var zadatak in zadaciF)
                zadaci.Add(ZadatakReportModel.FromZadatak(zadatak));

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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(zadaci));

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
                    column.PropertyName<ZadatakReportModel>(z => z.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakReportModel>(z => z.VrstaZadatka.NazivVrsteZdtk);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Vrsta zadatka", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakReportModel>(z => z.Zahtjev.Naziv);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Zahtjev", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakReportModel>(z => z.VrijemeIsporuke);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Vrijeme isporuke", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakReportModel>(z => z.StupanjDovrsenosti);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Stupanj dovršenosti", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakReportModel>(z => z.Prioritetnost);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Prioritetnost", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=poslovi.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Akcija za generiranje PDF izvještaja o vrstama zadataka.
        /// </summary>
        /// <returns>Rezultat akcije koji predstavlja PDF izvještaj o vrstama zadataka.</returns>
        public async Task<IActionResult> VrstaZadatka()
        {
            string naslov = "Popis vrsta zadataka";

            var vrstaZadatka = await ctx.VrstaZadatka
                .AsNoTracking()
                .OrderBy(vz => vz.NazivVrsteZdtk)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(vrstaZadatka));

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
                    column.PropertyName<VrstaZadatka>(vz => vz.NazivVrsteZdtk);
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
                Response.Headers.Add("content-disposition", "inline; filename=VrstaZadatka.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Akcija za generiranje PDF izvještaja o osobama.
        /// </summary>
        /// <returns>Rezultat akcije koji predstavlja PDF izvještaj o osobama.</returns>
        public async Task<IActionResult> Osoba()
        {
            string naslov = "Popis osoba";
            var osobe = await ctx.Osoba
                .AsNoTracking()
                .OrderBy(d => d.IdOsoba)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(osobe));

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
                    column.PropertyName<Osoba>(o => o.ImeOsobe);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.PrezimeOsobe);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.Email);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("E-mail", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.Telefon);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Telefon", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Osoba>(o => o.Iban);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("IBAN", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=osobe.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Akcija za generiranje PDF izvještaja o ulogama.
        /// </summary>
        /// <returns>Rezultat akcije koji predstavlja PDF izvještaj o ulogama.</returns>
        public async Task<IActionResult> Uloga()
        {
            string naslov = "Uloge";

            var uloge = await ctx.Uloga
                .AsNoTracking()
                .OrderBy(u => u.NazivUloge)
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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(uloge));

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
                    column.PropertyName<Uloga>(u => u.NazivUloge);
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
                Response.Headers.Add("content-disposition", "inline; filename=uloge.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Stvara PDF izvještaj s zadanim postavkama.
        /// </summary>
        /// <param name="naslov">Naziv izvještaja.</param>
        /// <returns>Instanca klase PdfReport s postavljenim parametrima.</returns>
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

        /// <summary>
        /// Generira PDF izvještaj sa zadacima u MD prikazu.
        /// </summary>
        /// <returns>PDF datoteka sa zadacima u MD prikazu.</returns>
        public async Task<IActionResult> MDZadaci()
        {
            string naslov = "Mirta MD zadaci";

            var zadaci = await ctx.Zadatak
                .Include(z => z.VrstaZadatka)
                .Include(z => z.Zahtjev)
                .Include(z => z.ZaduzenaOsoba)
                .ToListAsync();

            List<ZadatakOsoba> zadatakMD = new List<ZadatakOsoba>();
            foreach (var z in zadaci)
            {
                List<ZaduzenaOsoba> osobe = await ctx.ZaduzenaOsoba
                    .Where(s => s.IdZadatak == z.IdZadatak)
                    .Include(s => s.Osoba)
                    .ToListAsync();

                Boolean zeroAdded = osobe.IsNullOrEmpty();

                foreach (var osoba in osobe)
                {
                    var dto = new ZadatakOsoba();
                    dto.Zadatak = z;
                    dto.Osoba = osoba.Osoba;
                    zadatakMD.Add(dto);
                }

                if (zeroAdded)
                {
                    ZadatakOsoba newMD = new ZadatakOsoba();
                    newMD.Zadatak = z;
                    Osoba o = new Osoba();
                    newMD.Osoba = o;
                    zadatakMD.Add(newMD);
                }

            }

            zadatakMD
                .OrderBy(z => z.Zadatak.IdZadatak)
                .OrderBy(z => z.Osoba.ImeOsobe)
                .OrderBy(z => z.Osoba.PrezimeOsobe);

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
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(zadatakMD));

            report.MainTableColumns(columns =>
            {
                #region Stupci po kojima se grupira
                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakMDV2ViewModel>(z => z.Zadatak.IdZadatak);
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
                    column.PropertyName<ZadatakOsoba>(s => s.Osoba.ImeOsobe);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Ime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakOsoba>(s => s.Osoba.PrezimeOsobe);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Prezime", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakOsoba>(s => s.Osoba.Email);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("E-mail", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakOsoba>(s => s.Osoba.Telefon);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Telefon", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<ZadatakOsoba>(s => s.Osoba.Iban);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("IBAN", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion
            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=ZadatakMD.pdf");
                return File(pdf, "application/pdf");
            }
            else
                return NotFound();
        }

        #region Export u Excel
        /// <summary>
        /// Generira Excel datoteku s popisom zadataka u jednostavnom formatu.
        /// </summary>
        /// <returns>Excel datoteka s popisom zadataka.</returns>
        public async Task<IActionResult> ExcelSimpleZadatak()
        {
            var zadatak = await ctx.Zadatak
                                  .Include(p => p.VrstaZadatka)
                                  .Include(p => p.Zahtjev)
                                  .AsNoTracking()
                                  .OrderBy(p => p.IdZadatak)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis zadataka";
                excel.Workbook.Properties.Author = "Mirta zadaci";
                var worksheet = excel.Workbook.Worksheets.Add("Zadaci");

                //First add the headers
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Naziv";
                worksheet.Cells[1, 3].Value = "Vrsta zadatka";
                worksheet.Cells[1, 4].Value = "Zahtjev";
                worksheet.Cells[1, 5].Value = "Vrijeme isporuke";
                worksheet.Cells[1, 6].Value = "Stupanj dovršenosti";
                worksheet.Cells[1, 7].Value = "Prioritetnost";

                for (int i = 0; i < zadatak.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = zadatak[i].IdZadatak;
                    worksheet.Cells[i + 2, 2].Value = zadatak[i].Naziv;
                    worksheet.Cells[i + 2, 3].Value = zadatak[i].VrstaZadatka.NazivVrsteZdtk;
                    worksheet.Cells[i + 2, 4].Value = zadatak[i].Zahtjev.Naziv;
                    worksheet.Cells[i + 2, 5].Value = zadatak[i].VrijemeIsporuke.ToString("dd.MM.yyyy.");
                    worksheet.Cells[i + 2, 6].Value = zadatak[i].StupanjDovrsenosti;
                    worksheet.Cells[i + 2, 7].Value = zadatak[i].Prioritetnost;
                }

                worksheet.Cells[1, 1, zadatak.Count + 1, 11].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "zadaci.xlsx");
        }

        /// <summary>
        /// Generira Excel datoteku s popisom vrsta zadataka u jednostavnom formatu.
        /// </summary>
        /// <returns>Excel datoteka s popisom vrsta zadataka.</returns>
        public async Task<IActionResult> ExcelSimpleVrsteZadataka()
        {
            var vrsteZadataka = await ctx.VrstaZadatka
                                    .AsNoTracking()
                                    .OrderBy(vp => vp.IdVrstaZdtk)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis vrsta zadataka";
                excel.Workbook.Properties.Author = "Mirta vrste zadataka";
                var worksheet = excel.Workbook.Worksheets.Add("Vrste zadataka");

                //First add the headers
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Naziv";

                for (int i = 0; i < vrsteZadataka.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = vrsteZadataka[i].IdVrstaZdtk;
                    worksheet.Cells[i + 2, 2].Value = vrsteZadataka[i].NazivVrsteZdtk;
                }

                worksheet.Cells[1, 1, vrsteZadataka.Count + 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "vrsteZadataka.xlsx");
        }

        /// <summary>
        /// Generira Excel datoteku s popisom osoba u jednostavnom formatu.
        /// </summary>
        /// <returns>Excel datoteka s popisom osoba.</returns>
        public async Task<IActionResult> ExcelSimpleOsoba()
        {
            var osobe = await ctx.Osoba
                                    .AsNoTracking()
                                    .OrderBy(s => s.IdOsoba)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis osoba";
                excel.Workbook.Properties.Author = "Mirta osobe";
                var worksheet = excel.Workbook.Worksheets.Add("Osobe");

                //First add the headers
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Ime";
                worksheet.Cells[1, 3].Value = "Prezime";
                worksheet.Cells[1, 4].Value = "E-mail";
                worksheet.Cells[1, 5].Value = "Telefon";
                worksheet.Cells[1, 6].Value = "IBAN";

                for (int i = 0; i < osobe.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = osobe[i].IdOsoba;
                    worksheet.Cells[i + 2, 2].Value = osobe[i].ImeOsobe;
                    worksheet.Cells[i + 2, 3].Value = osobe[i].PrezimeOsobe;
                    worksheet.Cells[i + 2, 4].Value = osobe[i].Email;
                    worksheet.Cells[i + 2, 5].Value = osobe[i].Telefon;
                    worksheet.Cells[i + 2, 6].Value = osobe[i].Iban;
                }
                worksheet.Cells[1, 1, osobe.Count + 1, 9].AutoFitColumns();
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "osobe.xlsx");
        }

        /// <summary>
        /// Generira Excel datoteku s popisom uloga u jednostavnom formatu.
        /// </summary>
        /// <returns>Excel datoteka s popisom uloga.</returns>
        public async Task<IActionResult> ExcelSimpleUloga()
        {
            var uloge = await ctx.Uloga
                                    .AsNoTracking()
                                    .OrderBy(vs => vs.IdUloga)
                                    .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis uloga";
                excel.Workbook.Properties.Author = "Mirta uloge";
                var worksheet = excel.Workbook.Worksheets.Add("Uloge");

                //First add the headers
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Naziv";

                for (int i = 0; i < uloge.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = uloge[i].IdUloga;
                    worksheet.Cells[i + 2, 2].Value = uloge[i].NazivUloge;
                }

                worksheet.Cells[1, 1, uloge.Count + 1, 5].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "uloge.xlsx");
        }

        /// <summary>
        /// Generira Excel datoteku s popisom zadataka u kompleksnom (MD) formatu.
        /// </summary>
        /// <returns>Excel datoteka s popisom zadataka.</returns>
        public async Task<IActionResult> ExcelComplexZadatak()
        {
            var zadatak = await ctx.Zadatak
                .Include(p => p.VrstaZadatka).Include(p => p.Zahtjev).
                OrderBy(p => p.IdZadatak).ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "MD zadaci";
                excel.Workbook.Properties.Author = "Mirta MD zadaci";
                var worksheet = excel.Workbook.Worksheets.Add("MD zadaci");
                worksheet.Cells[2, 1].Value = "Zadatak";
                worksheet.Cells[4, 1].Value = "Osobe";
                worksheet.Cells[2, 1].AutoFitColumns();
                worksheet.Cells[4, 1].AutoFitColumns();


                //First add the headers
                for (int i = 0; i < zadatak.Count; i++)
                {
                    worksheet.Cells[1, i * 11 + 2].Value = "ID";
                    worksheet.Cells[1, i * 11 + 3].Value = "Naziv";
                    worksheet.Cells[1, i * 11 + 4].Value = "Vrsta zadatka";
                    worksheet.Cells[1, i * 11 + 5].Value = "Zahtjev";
                    worksheet.Cells[1, i * 11 + 6].Value = "Vrijeme isporuke";
                    worksheet.Cells[1, i * 11 + 7].Value = "Stupanj dovršenosti";
                    worksheet.Cells[1, i * 11 + 8].Value = "Prioritetnost";
                    worksheet.Cells[1, i * 11 + 9].Value = "|";
                    worksheet.Cells[2, i * 11 + 2].Value = zadatak[i].IdZadatak;
                    worksheet.Cells[2, i * 11 + 3].Value = zadatak[i].Naziv;
                    worksheet.Cells[2, i * 11 + 4].Value = zadatak[i].VrstaZadatka.NazivVrsteZdtk;
                    worksheet.Cells[2, i * 11 + 5].Value = zadatak[i].Zahtjev.Naziv;
                    worksheet.Cells[2, i * 11 + 6].Value = zadatak[i].VrijemeIsporuke.ToString("dd.MM.yyyy.");
                    worksheet.Cells[2, i * 11 + 7].Value = zadatak[i].StupanjDovrsenosti;
                    worksheet.Cells[2, i * 11 + 8].Value = zadatak[i].Prioritetnost;
                    worksheet.Cells[2, i * 11 + 9].Value = "|";

                    var osobe = await ctx.ZaduzenaOsoba.Where(s => s.IdZadatak == zadatak[i].IdZadatak).Include(s => s.Osoba).ToListAsync();
                    worksheet.Cells[4, i * 11 + 2].Value = "ID";
                    worksheet.Cells[4, i * 11 + 3].Value = "Ime";
                    worksheet.Cells[4, i * 11 + 4].Value = "Prezime";
                    worksheet.Cells[4, i * 11 + 5].Value = "E-mail";
                    worksheet.Cells[4, i * 11 + 6].Value = "Telefon";
                    worksheet.Cells[4, i * 11 + 7].Value = "IBAN";
                    worksheet.Cells[4, i * 11 + 2].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 3].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 4].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 5].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 6].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 7].AutoFitColumns();
                    for (int j = 0; j < osobe.Count; j++)
                    {
                        worksheet.Cells[j + 5, i * 11 + 2].Value = osobe[j].IdOsoba;
                        worksheet.Cells[j + 5, i * 11 + 3].Value = osobe[j].Osoba.ImeOsobe;
                        worksheet.Cells[j + 5, i * 11 + 4].Value = osobe[j].Osoba.PrezimeOsobe;
                        worksheet.Cells[j + 5, i * 11 + 5].Value = osobe[j].Osoba.Email;
                        worksheet.Cells[j + 5, i * 11 + 6].Value = osobe[j].Osoba.Telefon;
                        worksheet.Cells[j + 5, i * 11 + 7].Value = osobe[j].Osoba.Iban;
                        worksheet.Cells[j + 5, i * 11 + 2].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 3].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 5].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 6].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 7].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 4].AutoFitColumns();
                    }
                }

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "Zadaci.xlsx");
        }

        #endregion

        #region Master-detail header
        /// <summary>
        /// Implementacija zaglavlja stranice za prikaz u izvješću u MD formatu.
        /// </summary>
        public class MasterDetailsHeaders : IPageHeader
        {
            private string naslov;

            /// <summary>
            /// Konstruktor klase MasterDetailsHeaders.
            /// </summary>
            /// <param name="naslov">Naslov koji će se prikazati u zaglavlju.</param>
            public MasterDetailsHeaders(string naslov)
            {
                this.naslov = naslov;
            }

            /// <summary>
            /// Font koji se koristi za prikaz teksta u zaglavlju.
            /// </summary>
            public IPdfFont PdfRptFont { set; get; }

            /// <summary>
            /// Metoda za renderiranje zaglavlja grupe u izvješću.
            /// </summary>
            /// <param name="pdfDoc">PDF dokument.</param>
            /// <param name="pdfWriter">Autor PDF-a.</param>
            /// <param name="newGroupInfo">Informacije o novoj grupi.</param>
            /// <param name="summaryData">Podaci o sažetku.</param>
            /// <returns>PDF tablica koja predstavlja zaglavlje grupe.</returns>
            public PdfGrid RenderingGroupHeader(Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo, IList<SummaryCellData> summaryData)
            {
                var zadatak = (Zadatak)newGroupInfo.GetValueOf(nameof(ZadatakOsoba.Zadatak));


                var table = new PdfGrid(relativeWidths: new[] { 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f, 2f, 4f }) { WidthPercentage = 100 };

                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "ID:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = zadatak.IdZadatak;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Naziv:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = zadatak.Naziv;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Vrsta zadatka";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = zadatak.VrstaZadatka.NazivVrsteZdtk;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Zahtjev";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = zadatak.Zahtjev.Naziv;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                (cellData, cellProperties) => {
                    cellData.Value = "Vrijeme isporuke";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = zadatak.VrijemeIsporuke.ToString("dd.MM.yyyy.");
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) => {
                    cellData.Value = "Stupanj dovršenosti";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = zadatak.StupanjDovrsenosti;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) => {
                    cellData.Value = "Prioritetnost";
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                },
                (cellData, cellProperties) =>
                {
                    cellData.Value = zadatak.Prioritetnost;
                    cellProperties.PdfFont = PdfRptFont;
                    cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                });
                return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
            }

            /// <summary>
            /// Metoda za renderiranje zaglavlja izvješća.
            /// </summary>
            /// <param name="pdfDoc">PDF dokument.</param>
            /// <param name="pdfWriter">Autor PDF-a.</param>
            /// <param name="summaryData">Podaci o sažetku.</param>
            /// <returns>PDF tablica koja predstavlja zaglavlje izvješća.</returns>
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
