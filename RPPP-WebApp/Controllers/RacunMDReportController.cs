using System.Linq.Expressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers
{
    public class RacunMDReportController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vpd.openxmlformats-officedocument.spreadsheetml.sheet";

        public RacunMDReportController(RPPP09Context ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Racun()
        {
            string naslov = "Popis računa";
            var racuniF = await ctx.Racun
                .Include(p => p.IdValutaNavigation)
                .AsNoTracking()
                .OrderBy(p => p)
                .ToListAsync();

            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(racuniF));

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
                    column.PropertyName<Racun>(p => p.ImeRacuna);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Naziv", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Racun>(p => p.Iban);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("IBAN", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<Racun>(p => p.IdValutaNavigation.IsoOznaka);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Valuta", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=racuni.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Valuta()
        {
            string naslov = "Popis valuta";
            var valuta = await ctx.Valuta
                .AsNoTracking()
                .OrderBy(vp => vp.IsoOznaka)
                .ToListAsync();
            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(valuta));

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
                    column.PropertyName<Valuta>(vp => vp.IsoOznaka);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Oznaka", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=valute.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Transakcija()
        {
            string naslov = "Popis transakcija";
            var transakcije = await ctx.Transakcija
                .AsNoTracking()
                .Include(s => s.VrstaTransakcije)
                .Include(s => s.Racun)
                .Include(s => s.UnutarnjiRacun)
                .OrderBy(d => d.Idtransakcije)
                .ToListAsync();

            var reportTransakcije = transakcije.Select(
                transakcija => TransakcijaReportModel.FromTransakcija(transakcija)).ToList();


            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(reportTransakcije));

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
                    column.PropertyName<TransakcijaReportModel>(s => s.imeRacuna);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Racun", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<TransakcijaReportModel>(s => s.primateljPosiljatelj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Primatelj/pošiljatelj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<TransakcijaReportModel>(s => s.iznos);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Iznos", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<TransakcijaReportModel>(s => s.vrsta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Vrsta", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<TransakcijaReportModel>(s => s.unutarnjaVanjska);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Unutarnja/vanjska", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=transakcije.pdf");
                return File(pdf, "application/pdf");
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> VrstaTransakcije()
        {
            string naslov = "Vrste transakcija";
            var kategorije = await ctx.VrstaTransakcije
                .AsNoTracking()
                .OrderBy(vs => vs.Naziv)
                .ToListAsync();
            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
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
                    column.PropertyName<VrstaTransakcije>(vs => vs.Naziv);
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
                Response.Headers.Add("content-disposition", "inline; filename=vrsteTransakcija.pdf");
                return File(pdf, "application/pdf");
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
                .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ProfessionalTemplate); })
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

        public async Task<IActionResult> MDRacuni()
        {
            string naslov = "MD računi";
            var računi = await ctx.Racun.Include(p => p.IdValutaNavigation)
                .Include(p => p.TransakcijaIdracunaNavigation).ToListAsync();
            List<RacunTransakcijaReportModel> racunMD = new List<RacunTransakcijaReportModel>();
            foreach (var p in računi)
            {
                List<TransakcijaReportModel> transakcije = await ctx.Transakcija.Where(s => s.Idracuna == p.IdRacuna)
                    .Include(s => s.VrstaTransakcije)
                    .Include(s => s.UnutarnjiRacun)
                    .Select(s => TransakcijaReportModel.FromTransakcija(s)).ToListAsync();
                Boolean zeroAdded = transakcije.IsNullOrEmpty();

                foreach (var transakcija in transakcije)
                {
                    var dto = new RacunTransakcijaReportModel();
                    dto.Racun = p;
                    dto.Transakcija = transakcija;
                    racunMD.Add(dto);
                }

                if (zeroAdded)
                {
                    RacunTransakcijaReportModel newMD = new RacunTransakcijaReportModel();
                    newMD.Racun = p;
                    Transakcija s = new Transakcija();
                    s.VrstaTransakcije = new VrstaTransakcije();
                    s.UnutarnjiRacun = new Racun();
                    s.Racun = p;
                    newMD.Transakcija = TransakcijaReportModel.FromTransakcija(s);
                    racunMD.Add(newMD);
                }
            }

            var ordered = racunMD.OrderBy(km => km.Racun.IdRacuna).ThenBy(km => km.Transakcija.Idtransakcije);
            PdfReport report = CreateReport(naslov);

            #region Podnožje i zaglavlje

            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy.")); })
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

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(ordered));

            report.MainTableColumns(columns =>
            {
                #region Stupci po kojima se grupira

                columns.AddColumn(column =>
                {
                    column.PropertyName<RacunMDViewModel>(km => km.Racun.IdRacuna);
                    column.Group(
                        (val1, val2) => (int)val1 == (int)val2);
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
                    column.PropertyName<RacunTransakcijaReportModel>(s => s.Racun.ImeRacuna);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Račun", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<RacunTransakcijaReportModel>(s => s.Transakcija.primateljPosiljatelj);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Primatelj/pošiljatelj", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<RacunTransakcijaReportModel>(s => s.Transakcija.iznos);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(1);
                    column.HeaderCell("Iznos", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<RacunTransakcijaReportModel>(s => s.Transakcija.vrsta);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Vrsta transakcije", horizontalAlignment: HorizontalAlignment.Center);
                });


                columns.AddColumn(column =>
                {
                    column.PropertyName<RacunTransakcijaReportModel>(s => s.Transakcija.unutarnjaVanjska);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(1);
                    column.HeaderCell("Unutarnja/vanjska", horizontalAlignment: HorizontalAlignment.Center);
                });
            });

            #endregion

            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=MDRacuni.pdf");
                return File(pdf, "application/pdf");
            }
            else
                return NotFound();
        }

        #region Export u Excel

        public async Task<IActionResult> ExcelSimpleRacun()
        {
            var racun = await ctx.Racun
                .Include(p => p.IdValutaNavigation)
                .AsNoTracking()
                .OrderBy(p => p.IdRacuna)
                .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis računa";
                excel.Workbook.Properties.Author = "RPPP09";
                var worksheet = excel.Workbook.Worksheets.Add("Računi");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv";
                worksheet.Cells[1, 3].Value = "IBAN";
                worksheet.Cells[1, 4].Value = "Valuta";

                for (int i = 0; i < racun.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = racun[i].IdRacuna;
                    worksheet.Cells[i + 2, 2].Value = racun[i].ImeRacuna;
                    worksheet.Cells[i + 2, 3].Value = racun[i].Iban;
                    worksheet.Cells[i + 2, 4].Value = racun[i].IdValutaNavigation.IsoOznaka;
                }

                worksheet.Cells[1, 1, racun.Count + 1, 11].AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "racuni.xlsx");
        }

        public async Task<IActionResult> ExcelSimpleValuta()
        {
            var valute = await ctx.Valuta
                .AsNoTracking()
                .OrderBy(vp => vp.Idvaluta)
                .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis valuta";
                excel.Workbook.Properties.Author = "RPPP09";
                var worksheet = excel.Workbook.Worksheets.Add("Valute");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "ISO oznaka";

                for (int i = 0; i < valute.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = valute[i].Idvaluta;
                    worksheet.Cells[i + 2, 2].Value = valute[i].IsoOznaka;
                }

                worksheet.Cells[1, 1, valute.Count + 1, 9].AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "valute.xlsx");
        }

        public async Task<IActionResult> ExcelSimpleTransakcija()
        {
            var transakcijeRaw = await ctx.Transakcija
                .Include(s => s.VrstaTransakcije)
                .Include(s => s.Racun)
                .Include(s => s.UnutarnjiRacun)
                .AsNoTracking()
                .OrderBy(s => s.Idtransakcije)
                .ToListAsync();

            var transakcije = transakcijeRaw.Select(s => TransakcijaReportModel.FromTransakcija(s)).ToList();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis transakcija";
                excel.Workbook.Properties.Author = "RPPP09";
                var worksheet = excel.Workbook.Worksheets.Add("Transakcije");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Račun";
                worksheet.Cells[1, 3].Value = "Primatelj/pošiljatelj";
                worksheet.Cells[1, 4].Value = "Iznos";
                worksheet.Cells[1, 5].Value = "Vrsta";
                worksheet.Cells[1, 6].Value = "Unutarnja/vanjska";

                for (int i = 0; i < transakcije.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = transakcije[i].Idtransakcije;
                    worksheet.Cells[i + 2, 2].Value = transakcije[i].imeRacuna;
                    worksheet.Cells[i + 2, 3].Value = transakcije[i].primateljPosiljatelj;
                    worksheet.Cells[i + 2, 4].Value = transakcije[i].iznos;
                    worksheet.Cells[i + 2, 5].Value = transakcije[i].vrsta;
                    worksheet.Cells[i + 2, 6].Value = transakcije[i].unutarnjaVanjska;

                    worksheet.Cells[1, 1, transakcije.Count + 1, 9].AutoFitColumns();
                }

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "transakcije.xlsx");
        }

        public async Task<IActionResult> ExcelSimpleVrstaTransakcije()
        {
            var vrsteTransakcija = await ctx.VrstaTransakcije
                .AsNoTracking()
                .OrderBy(vs => vs.IdvrstaTransakcije)
                .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis vrsta transakcija";
                excel.Workbook.Properties.Author = "RPPP09";
                var worksheet = excel.Workbook.Worksheets.Add("Vrste transakcija");

                //First add the headers
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Naziv";

                for (int i = 0; i < vrsteTransakcija.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = vrsteTransakcija[i].IdvrstaTransakcije;
                    worksheet.Cells[i + 2, 2].Value = vrsteTransakcija[i].Naziv;
                }

                worksheet.Cells[1, 1, vrsteTransakcija.Count + 1, 5].AutoFitColumns();

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "vrsteTransakcija.xlsx");
        }

        public async Task<IActionResult> ExcelComplexRacun()
        {
            var racun = await ctx.Racun.Include(p => p.IdValutaNavigation).OrderBy(p => p.IdRacuna)
                .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "MD računi";
                excel.Workbook.Properties.Author = "RPPP09";
                var worksheet = excel.Workbook.Worksheets.Add("MD računi");
                worksheet.Cells[2, 1].Value = "Račun";
                worksheet.Cells[4, 1].Value = "Transakcije";
                worksheet.Cells[2, 1].AutoFitColumns();
                worksheet.Cells[4, 1].AutoFitColumns();


                //First add the headers
                for (int i = 0; i < racun.Count; i++)
                {
                    worksheet.Cells[1, i * 11 + 2].Value = "Id";
                    worksheet.Cells[1, i * 11 + 3].Value = "Naziv";
                    worksheet.Cells[1, i * 11 + 4].Value = "IBAN";
                    worksheet.Cells[1, i * 11 + 5].Value = "Valuta";
                    worksheet.Cells[1, i * 11 + 6].Value = "|";
                    worksheet.Cells[2, i * 11 + 2].Value = racun[i].IdRacuna;
                    worksheet.Cells[2, i * 11 + 3].Value = racun[i].ImeRacuna;
                    worksheet.Cells[2, i * 11 + 4].Value = racun[i].Iban;
                    worksheet.Cells[2, i * 11 + 5].Value = racun[i].IdValutaNavigation.IsoOznaka;
                    worksheet.Cells[2, i * 11 + 6].Value = "|";

                    var indexRacuna = i;
                    var transakcije = await ctx.Transakcija.Where(s => s.Idracuna == racun[indexRacuna].IdRacuna)
                        .Include(s => s.VrstaTransakcije).Include(s => s.UnutarnjiRacun)
                        .Select(s => TransakcijaReportModel.FromTransakcija(s)).ToListAsync();
                    worksheet.Cells[4, i * 11 + 2].Value = "Id";
                    worksheet.Cells[4, i * 11 + 3].Value = "Primatelj/pošiljatelj";
                    worksheet.Cells[4, i * 11 + 4].Value = "Iznos";
                    worksheet.Cells[4, i * 11 + 5].Value = "Vrsta";
                    worksheet.Cells[4, i * 11 + 6].Value = "Unutarnja/vanjska";
                    worksheet.Cells[4, i * 11 + 2].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 3].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 4].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 5].AutoFitColumns();
                    worksheet.Cells[4, i * 11 + 6].AutoFitColumns();
                    for (int j = 0; j < transakcije.Count; j++)
                    {
                        worksheet.Cells[j + 5, i * 11 + 2].Value = transakcije[j].Idtransakcije;
                        worksheet.Cells[j + 5, i * 11 + 3].Value = transakcije[j].primateljPosiljatelj;
                        worksheet.Cells[j + 5, i * 11 + 4].Value = transakcije[j].iznos;
                        worksheet.Cells[j + 5, i * 11 + 5].Value = transakcije[j].vrsta;
                        worksheet.Cells[j + 5, i * 11 + 6].Value = transakcije[j].unutarnjaVanjska;
                        worksheet.Cells[j + 5, i * 11 + 2].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 3].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 4].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 5].AutoFitColumns();
                        worksheet.Cells[j + 5, i * 11 + 6].AutoFitColumns();
                    }
                }

                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "MDRacuni.xlsx");
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

            public PdfGrid RenderingGroupHeader(Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo,
                IList<SummaryCellData> summaryData)
            {
                var racun = (Racun)newGroupInfo.GetValueOf(nameof(RacunTransakcijaReportModel.Racun));


                var table = new PdfGrid(
                        relativeWidths: new[] { 1f, 3f, 2f, 5f, 2f, 5f, 2f, 1f })
                    { WidthPercentage = 100 };

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
                        cellData.Value = racun.IdRacuna;
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
                        cellData.Value = racun.ImeRacuna;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "IBAN";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = racun.Iban;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Valuta";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = racun.IdValutaNavigation.IsoOznaka;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    });
                return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
            }

            public PdfGrid RenderingReportHeader(Document pdfDoc, PdfWriter pdfWriter,
                IList<SummaryCellData> summaryData)
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