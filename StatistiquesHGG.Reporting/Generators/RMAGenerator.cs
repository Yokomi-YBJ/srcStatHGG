using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StatistiquesHGG.Core.Enums;
using StatistiquesHGG.Infrastructure.Data;

namespace StatistiquesHGG.Reporting.Generators;

public class RMAGenerator
{
    private readonly AppDbContext _context;

    public RMAGenerator(AppDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenererExcelAsync(DateTime periode, string dossierSortie, DateTime? dateFin = null)
    {
        var debut = new DateTime(periode.Year, periode.Month, 1);
        var fin = dateFin ?? debut.AddMonths(1).AddDays(-1);
        var nomMois = fin.Month == debut.Month ? debut.ToString("MMMM yyyy") : $"{debut:MMM yyyy} — {fin:MMM yyyy}";

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("RMA");

        // Header style
        var headerStyle = workbook.Style;

        // Title
        ws.Range("A1:K1").Merge();
        ws.Cell("A1").Value = $"RAPPORT MENSUEL D'ACTIVITÉS - {nomMois.ToUpper()}";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 91, 161); // HGG Blue

        ws.Range("A2:K2").Merge();
        ws.Cell("A2").Value = "HÔPITAL GÉNÉRAL DE GAROUA";
        ws.Cell("A2").Style.Font.Bold = true;
        ws.Cell("A2").Style.Font.FontSize = 12;
        ws.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Section 1: Consultations Externes
        int row = 4;
        ws.Cell(row, 1).Value = "SECTION 1 : CONSULTATIONS EXTERNES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 164, 189);
        ws.Range(row, 1, row, 11).Merge();
        row++;

        // Headers
        ws.Cell(row, 1).Value = "Service";
        ws.Cell(row, 2).Value = "Nouv. H";
        ws.Cell(row, 3).Value = "Nouv. F";
        ws.Cell(row, 4).Value = "Nouv. E";
        ws.Cell(row, 5).Value = "Total Nouv.";
        ws.Cell(row, 6).Value = "Anc. H";
        ws.Cell(row, 7).Value = "Anc. F";
        ws.Cell(row, 8).Value = "Anc. E";
        ws.Cell(row, 9).Value = "Total Anc.";
        ws.Cell(row, 10).Value = "TOTAL";

        for (int c = 1; c <= 10; c++)
        {
            ws.Cell(row, c).Style.Font.Bold = true;
            ws.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromArgb(224, 242, 254);
            ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
        row++;

        // Consultations data
        var services = await _context.Services.Where(s => s.Actif).OrderBy(s => s.Libelle).ToListAsync();
        int totalConsNouv = 0, totalConsAnc = 0;

        foreach (var service in services)
        {
            var consults = await _context.SaisiesConsultation
                .Where(c => c.ServiceId == service.Id && c.DateSaisie >= debut && c.DateSaisie <= fin && c.Validee)
                .ToListAsync();

            if (!consults.Any()) continue;

            var nouvH = consults.Sum(c => c.NouveauxHommes);
            var nouvF = consults.Sum(c => c.NouvellesFemmes);
            var nouvE = consults.Sum(c => c.NouveauxEnfants);
            var ancH = consults.Sum(c => c.AnciensHommes);
            var ancF = consults.Sum(c => c.AnciennesFemmes);
            var ancE = consults.Sum(c => c.AnciensEnfants);

            ws.Cell(row, 1).Value = service.Libelle;
            ws.Cell(row, 2).Value = nouvH;
            ws.Cell(row, 3).Value = nouvF;
            ws.Cell(row, 4).Value = nouvE;
            ws.Cell(row, 5).Value = nouvH + nouvF + nouvE;
            ws.Cell(row, 6).Value = ancH;
            ws.Cell(row, 7).Value = ancF;
            ws.Cell(row, 8).Value = ancE;
            ws.Cell(row, 9).Value = ancH + ancF + ancE;
            ws.Cell(row, 10).Value = (nouvH + nouvF + nouvE) + (ancH + ancF + ancE);

            totalConsNouv += nouvH + nouvF + nouvE;
            totalConsAnc += ancH + ancF + ancE;

            for (int c = 1; c <= 10; c++)
                ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        // Total row
        ws.Cell(row, 1).Value = "TOTAL GÉNÉRAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = totalConsNouv;
        ws.Cell(row, 9).Value = totalConsAnc;
        ws.Cell(row, 10).Value = totalConsNouv + totalConsAnc;
        ws.Cell(row, 10).Style.Font.Bold = true;
        ws.Range(row, 1, row, 10).Style.Fill.BackgroundColor = XLColor.FromArgb(224, 242, 254);
        row += 2;

        // Section 2: Hospitalisations
        ws.Cell(row, 1).Value = "SECTION 2 : HOSPITALISATIONS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 164, 189);
        ws.Range(row, 1, row, 8).Merge();
        row++;

        ws.Cell(row, 1).Value = "Service";
        ws.Cell(row, 2).Value = "Hommes";
        ws.Cell(row, 3).Value = "Femmes";
        ws.Cell(row, 4).Value = "Enfants";
        ws.Cell(row, 5).Value = "Total";
        ws.Cell(row, 6).Value = "Jours Hosp.";
        ws.Cell(row, 7).Value = "DMS";
        for (int c = 1; c <= 7; c++)
        {
            ws.Cell(row, c).Style.Font.Bold = true;
            ws.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromArgb(224, 242, 254);
        }
        row++;

        foreach (var service in services)
        {
            var hosps = await _context.SaisiesHospitalisation
                .Where(h => h.ServiceId == service.Id && h.DateSaisie >= debut && h.DateSaisie <= fin && h.Validee)
                .ToListAsync();
            if (!hosps.Any()) continue;

            var h = hosps.Sum(x => x.Hommes);
            var f = hosps.Sum(x => x.Femmes);
            var e = hosps.Sum(x => x.Enfants);
            var total = h + f + e;
            var joursTotal = hosps.Sum(x => x.JoursHospitalisation);
            var dms = total > 0 ? Math.Round((double)joursTotal / total, 1) : 0;

            ws.Cell(row, 1).Value = service.Libelle;
            ws.Cell(row, 2).Value = h;
            ws.Cell(row, 3).Value = f;
            ws.Cell(row, 4).Value = e;
            ws.Cell(row, 5).Value = total;
            ws.Cell(row, 6).Value = joursTotal;
            ws.Cell(row, 7).Value = dms;
            for (int c = 1; c <= 7; c++)
                ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;
        }

        // Auto-fit columns
        ws.Columns().AdjustToContents();

        // Save
        Directory.CreateDirectory(dossierSortie);
        var fileName = Path.Combine(dossierSortie, $"RMA_{periode:MM_yyyy}.xlsx");
        workbook.SaveAs(fileName);
        return fileName;
    }

    public async Task<string> GenererPDFAsync(DateTime periode, string dossierSortie, DateTime? dateFin = null)
    {
        var debut = new DateTime(periode.Year, periode.Month, 1);
        var fin = dateFin ?? debut.AddMonths(1).AddDays(-1);
        var nomMois = fin.Month == debut.Month ? debut.ToString("MMMM yyyy") : $"{debut:MMM yyyy} — {fin:MMM yyyy}";
        Directory.CreateDirectory(dossierSortie);
        var fileName = Path.Combine(dossierSortie, $"RMA_{periode:MM_yyyy}.pdf");

        var consultations = await _context.SaisiesConsultation
            .Include(c => c.Service)
            .Where(c => c.DateSaisie >= debut && c.DateSaisie <= fin && c.Validee)
            .ToListAsync();

        var hospitalisations = await _context.SaisiesHospitalisation
            .Include(h => h.Service)
            .Where(h => h.DateSaisie >= debut && h.DateSaisie <= fin && h.Validee)
            .ToListAsync();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("HÔPITAL GÉNÉRAL DE GAROUA")
                                .FontSize(14).Bold().FontColor(Color.FromHex("#005BA1"));
                            c.Item().Text($"Rapport Mensuel d'Activités — {nomMois}")
                                .FontSize(11).FontColor(Color.FromHex("#00A4BD"));
                        });
                        row.ConstantItem(80).Text("HGG").FontSize(20).Bold()
                            .FontColor(Color.FromHex("#005BA1")).AlignRight();
                    });
                    col.Item().PaddingTop(4).LineHorizontal(2).LineColor(Color.FromHex("#005BA1"));
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    // Consultations table
                    col.Item().Text("1. CONSULTATIONS EXTERNES").Bold().FontSize(11)
                        .FontColor(Color.FromHex("#005BA1"));
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(); cols.RelativeColumn(); cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn(); cols.RelativeColumn(); cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("Service").Bold().FontColor(Colors.White).FontSize(8);
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("N.H").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("N.F").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("N.E").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("Tot.N").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("A.H").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("A.F").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("A.E").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("Tot.A").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                            header.Cell().Background(Color.FromHex("#005BA1")).Text("TOTAL").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                        });

                        var byService = consultations.GroupBy(c => c.Service?.Libelle ?? "Inconnu").OrderBy(g => g.Key);
                        int idx = 0;
                        foreach (var grp in byService)
                        {
                            var bg = idx % 2 == 0 ? Colors.White : Color.FromHex("#F0F9FF");
                            var nH = grp.Sum(c => c.NouveauxHommes);
                            var nF = grp.Sum(c => c.NouvellesFemmes);
                            var nE = grp.Sum(c => c.NouveauxEnfants);
                            var aH = grp.Sum(c => c.AnciensHommes);
                            var aF = grp.Sum(c => c.AnciennesFemmes);
                            var aE = grp.Sum(c => c.AnciensEnfants);

                            table.Cell().Background(bg).Text(grp.Key).FontSize(8);
                            table.Cell().Background(bg).Text(nH.ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bg).Text(nF.ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bg).Text(nE.ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bg).Text((nH + nF + nE).ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bg).Text(aH.ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bg).Text(aF.ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bg).Text(aE.ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bg).Text((aH + aF + aE).ToString()).AlignCenter().FontSize(8);
                            table.Cell().Background(bg).Text((nH + nF + nE + aH + aF + aE).ToString()).Bold().AlignCenter().FontSize(8);
                            idx++;
                        }
                    });
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                    // On définit d'abord l'item, puis l'alignement, et ENFIN le contenu Text
                    row.RelativeItem().AlignRight().Text(x => 
                    {
                        x.Span("Page ").FontSize(8);
                        x.CurrentPageNumber().FontSize(8);
                        x.Span(" / ").FontSize(8);
                        x.TotalPages().FontSize(8);
                    });
                });
            });
        }).GeneratePdf(fileName);

        return fileName;
    }
}
