using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Text;
using WorkflowsDemo.Events;
using WorkflowsDemo.Models.PlanBuilder;

namespace WorkflowsDemo.Executors;

internal sealed partial class PlanRendererExecutor(ILogger<PlanRendererExecutor> logger)
    : Executor(nameof(PlanRendererExecutor))
{
    private static int TripPlanIteration = 1;

    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.SendsMessage<PlanBuilderResult>();
        protocolBuilder.YieldsOutput<WorkflowCompletedSignal>();

        protocolBuilder.ConfigureRoutes(routeBuilder =>
        {
            routeBuilder.AddHandler<PlanBuilderResult>(HandleAsync);
        });

        return protocolBuilder;
    }

    private const string OutputFileNameBase = "trip-plan";
    private const string TripPlansDirectoryName = "trip-plans";

    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-US");

    private async ValueTask HandleAsync(
        PlanBuilderResult result,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(result.TripPlan);

        var outputFileName = $"{OutputFileNameBase}-v{TripPlanIteration}.html";
        TripPlanIteration++;

        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), TripPlansDirectoryName, outputFileName);
        var temporaryPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            $".{OutputFileNameBase}.{Guid.NewGuid():N}.tmp");
        
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        try
        {
            var html = RenderHtml(result.TripPlan);
            await File.WriteAllTextAsync(
                temporaryPath,
                html,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);

            File.Move(temporaryPath, outputPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        logger.LogInformation("Rendered visual trip itinerary");

        if (result.FinalPlanReady)
        {
            await context.YieldOutputAsync(new WorkflowCompletedSignal(), cancellationToken);
            return;
        }

        await context.SendMessageAsync(result, cancellationToken);
    }

    private static string RenderHtml(TripPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(plan.Summary);
        ArgumentNullException.ThrowIfNull(plan.Accommodation);

        var summary = plan.Summary;
        var accommodation = plan.Accommodation;
        var days = plan.Days ?? [];
        var warnings = plan.Warnings ?? [];
        var budgetUsage = summary.BudgetUsd > 0
            ? Math.Clamp(summary.TotalEstimatedCostUsd / summary.BudgetUsd * 100, 0, 100)
            : 0;

        var html = new StringBuilder(32_768);
        html.Append(
            """
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <meta name="color-scheme" content="light">
            """);
        html.Append("  <title>").Append(Encode(summary.Destination)).AppendLine(" trip plan</title>");
        html.Append(
            """
              <style>
                :root {
                  --ink: #17202a;
                  --muted: #667085;
                  --paper: #ffffff;
                  --canvas: #f4f7f6;
                  --line: #dce5e1;
                  --primary: #146b5d;
                  --primary-dark: #0b4c42;
                  --accent: #e6f4ef;
                  --warning: #8a4b08;
                  --warning-bg: #fff4dd;
                  --shadow: 0 18px 50px rgba(23, 32, 42, .09);
                  --radius: 22px;
                }

                * { box-sizing: border-box; }

                html { scroll-behavior: smooth; }

                body {
                  margin: 0;
                  color: var(--ink);
                  background:
                    radial-gradient(circle at 10% 0%, #dff3ec 0, transparent 30rem),
                    var(--canvas);
                  font: 16px/1.55 Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
                }

                a { color: var(--primary-dark); }

                .page {
                  width: min(1120px, calc(100% - 32px));
                  margin: 32px auto 64px;
                }

                .hero {
                  position: relative;
                  overflow: hidden;
                  padding: clamp(30px, 6vw, 68px);
                  color: #fff;
                  background: linear-gradient(135deg, #0b4c42, #178272);
                  border-radius: 28px;
                  box-shadow: var(--shadow);
                }

                .hero::after {
                  content: "";
                  position: absolute;
                  width: 320px;
                  height: 320px;
                  right: -90px;
                  top: -130px;
                  border: 56px solid rgba(255,255,255,.08);
                  border-radius: 50%;
                }

                .eyebrow {
                  margin: 0 0 8px;
                  font-size: .78rem;
                  font-weight: 800;
                  letter-spacing: .16em;
                  text-transform: uppercase;
                  opacity: .78;
                }

                h1 {
                  max-width: 820px;
                  margin: 0;
                  font-size: clamp(2.35rem, 7vw, 5rem);
                  line-height: .98;
                  letter-spacing: -.055em;
                }

                .hero-description {
                  max-width: 760px;
                  margin: 22px 0 0;
                  font-size: clamp(1rem, 2.3vw, 1.25rem);
                  color: rgba(255,255,255,.86);
                }

                .hero-meta {
                  display: flex;
                  flex-wrap: wrap;
                  gap: 10px;
                  margin-top: 28px;
                }

                .hero-meta span {
                  padding: 8px 12px;
                  background: rgba(255,255,255,.12);
                  border: 1px solid rgba(255,255,255,.18);
                  border-radius: 999px;
                  font-size: .9rem;
                  font-weight: 650;
                }

                .toolbar {
                  display: flex;
                  justify-content: flex-end;
                  flex-wrap: wrap;
                  gap: 8px;
                  margin: 18px 0;
                }

                button {
                  appearance: none;
                  padding: 9px 14px;
                  color: var(--primary-dark);
                  background: var(--paper);
                  border: 1px solid var(--line);
                  border-radius: 999px;
                  font: inherit;
                  font-size: .88rem;
                  font-weight: 700;
                  cursor: pointer;
                }

                button:hover { background: var(--accent); }
                button:focus-visible, a:focus-visible, summary:focus-visible {
                  outline: 3px solid #7bc8b9;
                  outline-offset: 3px;
                }

                .grid {
                  display: grid;
                  grid-template-columns: repeat(12, 1fr);
                  gap: 18px;
                }

                .card {
                  padding: clamp(22px, 3.5vw, 34px);
                  background: var(--paper);
                  border: 1px solid rgba(220,229,225,.8);
                  border-radius: var(--radius);
                  box-shadow: 0 8px 26px rgba(23,32,42,.055);
                }

                .overview { grid-column: span 7; }
                .budget { grid-column: span 5; }
                .accommodation, .warnings, .explanation { grid-column: 1 / -1; }

                .section-label {
                  margin: 0 0 5px;
                  color: var(--primary);
                  font-size: .75rem;
                  font-weight: 850;
                  letter-spacing: .13em;
                  text-transform: uppercase;
                }

                h2, h3, p { overflow-wrap: anywhere; }
                h2 { margin: 0 0 18px; font-size: clamp(1.45rem, 3vw, 2rem); line-height: 1.16; }
                h3 { margin: 0; font-size: 1.08rem; line-height: 1.25; }

                .facts {
                  display: grid;
                  grid-template-columns: repeat(2, minmax(0, 1fr));
                  gap: 18px;
                  margin: 0;
                }

                .facts div { min-width: 0; }
                .facts dt { color: var(--muted); font-size: .8rem; font-weight: 700; text-transform: uppercase; letter-spacing: .06em; }
                .facts dd { margin: 4px 0 0; font-size: 1.02rem; font-weight: 750; }

                .budget-total {
                  margin: 2px 0 3px;
                  font-size: clamp(2rem, 5vw, 3.4rem);
                  font-weight: 850;
                  line-height: 1;
                  letter-spacing: -.045em;
                }

                .muted { color: var(--muted); }

                .progress {
                  height: 10px;
                  margin: 24px 0 10px;
                  overflow: hidden;
                  background: #e8efec;
                  border-radius: 999px;
                }

                .progress span {
                  display: block;
                  width: var(--progress);
                  height: 100%;
                  background: linear-gradient(90deg, #28a58e, #146b5d);
                  border-radius: inherit;
                }

                .budget-row {
                  display: flex;
                  justify-content: space-between;
                  gap: 12px;
                  font-size: .9rem;
                }

                .accommodation-layout {
                  display: grid;
                  grid-template-columns: minmax(0, .85fr) minmax(0, 1.15fr);
                  gap: 30px;
                }

                .price { margin: 8px 0 18px; font-size: 1.45rem; font-weight: 850; }

                .chips {
                  display: flex;
                  flex-wrap: wrap;
                  gap: 8px;
                  margin: 14px 0 0;
                  padding: 0;
                  list-style: none;
                }

                .chip {
                  padding: 6px 10px;
                  color: var(--primary-dark);
                  background: var(--accent);
                  border-radius: 999px;
                  font-size: .83rem;
                  font-weight: 700;
                }

                .reason-list, .warning-list { margin: 12px 0 0; padding-left: 1.25rem; }
                .reason-list li + li, .warning-list li + li { margin-top: 9px; }

                .map-link {
                  display: inline-block;
                  margin-top: 10px;
                  font-size: .86rem;
                  font-weight: 750;
                }

                .warnings {
                  border-color: #f0d59f;
                  background: var(--warning-bg);
                }

                .warnings .section-label, .warnings h2 { color: var(--warning); }

                .itinerary { margin-top: 34px; }
                .itinerary-heading { margin-bottom: 16px; padding: 0 4px; }

                .day {
                  margin-bottom: 14px;
                  background: var(--paper);
                  border: 1px solid var(--line);
                  border-radius: var(--radius);
                  box-shadow: 0 7px 22px rgba(23,32,42,.045);
                }

                .day > summary {
                  display: grid;
                  grid-template-columns: auto 1fr auto;
                  align-items: center;
                  gap: 16px;
                  padding: 20px 24px;
                  cursor: pointer;
                  list-style: none;
                  border-radius: inherit;
                }

                .day > summary::-webkit-details-marker { display: none; }
                .day > summary::after {
                  content: "+";
                  display: grid;
                  place-items: center;
                  width: 32px;
                  height: 32px;
                  color: var(--primary-dark);
                  background: var(--accent);
                  border-radius: 50%;
                  font-size: 1.25rem;
                  font-weight: 400;
                }
                .day[open] > summary::after { content: "\2212"; }

                .day-number {
                  display: grid;
                  place-items: center;
                  width: 48px;
                  height: 48px;
                  color: #fff;
                  background: var(--primary);
                  border-radius: 15px;
                  font-weight: 850;
                }

                .day-title span { display: block; margin-top: 3px; color: var(--muted); font-size: .88rem; }
                .day-content { padding: 0 24px 26px; }
                .day-summary { margin: 0 0 24px; padding: 15px 17px; color: #34433f; background: #f5f8f7; border-radius: 12px; }

                .timeline { position: relative; margin-left: 10px; }
                .timeline::before {
                  content: "";
                  position: absolute;
                  top: 14px;
                  bottom: 16px;
                  left: 8px;
                  width: 2px;
                  background: var(--line);
                }

                .activity {
                  --type-color: #607d8b;
                  position: relative;
                  display: grid;
                  grid-template-columns: 18px minmax(0, 1fr);
                  gap: 18px;
                  padding-bottom: 24px;
                }

                .activity:last-child { padding-bottom: 0; }
                .activity-dot {
                  z-index: 1;
                  width: 18px;
                  height: 18px;
                  margin-top: 4px;
                  background: var(--type-color);
                  border: 4px solid var(--paper);
                  border-radius: 50%;
                  box-shadow: 0 0 0 2px var(--type-color);
                }

                .activity-top {
                  display: flex;
                  align-items: flex-start;
                  justify-content: space-between;
                  gap: 16px;
                }

                .activity-time { color: var(--primary-dark); font-size: .86rem; font-weight: 800; white-space: nowrap; }
                .activity-description { margin: 8px 0 0; color: #4b5b57; }

                .activity-meta {
                  display: flex;
                  flex-wrap: wrap;
                  gap: 7px;
                  margin-top: 10px;
                }

                .activity-meta span {
                  padding: 4px 8px;
                  color: var(--muted);
                  background: #f4f7f6;
                  border-radius: 7px;
                  font-size: .78rem;
                  font-weight: 650;
                }

                .type-badge {
                  color: var(--type-color) !important;
                  background: color-mix(in srgb, var(--type-color) 11%, white) !important;
                }

                .type-arrival, .type-departure { --type-color: #6f42c1; }
                .type-accommodation-check-in, .type-accommodation-check-out { --type-color: #0b7285; }
                .type-attraction { --type-color: #d97706; }
                .type-restaurant { --type-color: #c2415d; }
                .type-break, .type-free-time { --type-color: #2f855a; }
                .type-transport { --type-color: #2563a8; }

                .empty {
                  margin: 0;
                  padding: 18px;
                  color: var(--muted);
                  background: #f5f8f7;
                  border-radius: 12px;
                  text-align: center;
                }

                .footer { margin-top: 24px; color: var(--muted); font-size: .82rem; text-align: center; }

                @media (max-width: 760px) {
                  .page { width: min(100% - 20px, 1120px); margin-top: 10px; }
                  .hero { border-radius: 22px; }
                  .overview, .budget { grid-column: 1 / -1; }
                  .accommodation-layout { grid-template-columns: 1fr; }
                  .toolbar { justify-content: center; }
                  .day > summary { grid-template-columns: auto 1fr; padding: 17px; }
                  .day > summary::after { display: none; }
                  .day-content { padding: 0 17px 22px; }
                  .activity-top { display: block; }
                  .activity-time { display: block; margin-bottom: 4px; }
                }

                @media print {
                  :root { --canvas: #fff; }
                  body { background: #fff; font-size: 11pt; }
                  .page { width: 100%; margin: 0; }
                  .hero { padding: 28px; background: #0b4c42 !important; print-color-adjust: exact; box-shadow: none; }
                  .toolbar, .footer { display: none; }
                  .card, .day { break-inside: avoid; box-shadow: none; }
                  .day { margin-top: 12px; }
                  .day > summary::after { display: none; }
                  .day-content { display: block !important; }
                  a { color: inherit; text-decoration: none; }
                }
              </style>
            </head>
            <body>
              <main class="page">
                <header class="hero">
                  <p class="eyebrow">Your trip plan</p>
            """);
        html.Append("      <h1>").Append(Encode(summary.Destination)).AppendLine("</h1>");
        html.Append("      <p class=\"hero-description\">").Append(Encode(summary.ShortDescription)).AppendLine("</p>");
        html.AppendLine("      <div class=\"hero-meta\">");
        html.Append("        <span>").Append(FormatDate(summary.ArrivalDateTime)).Append(" - ")
            .Append(FormatDate(summary.DepartureDateTime)).AppendLine("</span>");
        html.Append("        <span>").Append(Encode(summary.TripStyle)).AppendLine("</span>");
        html.Append("        <span>").Append(summary.NumberOfAdults.ToString(DisplayCulture)).Append(" adult")
            .Append(summary.NumberOfAdults == 1 ? string.Empty : "s");
        if (summary.NumberOfChildren > 0)
        {
            html.Append(" + ").Append(summary.NumberOfChildren.ToString(DisplayCulture)).Append(" child")
                .Append(summary.NumberOfChildren == 1 ? string.Empty : "ren");
        }

        html.AppendLine("</span>");
        html.Append(
            """
                  </div>
                </header>

                <nav class="toolbar" aria-label="Plan controls">
                  <button type="button" data-action="expand">Expand all days</button>
                  <button type="button" data-action="collapse">Collapse all days</button>
                  <button type="button" data-action="print">Print plan</button>
                </nav>

                <section class="grid" aria-label="Trip overview">
                  <article class="card overview">
                    <p class="section-label">Overview</p>
                    <h2>At a glance</h2>
                    <dl class="facts">
                      <div>
                        <dt>Arrival</dt>
            """);
        html.Append("            <dd>").Append(FormatDateTime(summary.ArrivalDateTime)).AppendLine("</dd>");
        html.Append(
            """
                      </div>
                      <div>
                        <dt>Departure</dt>
            """);
        html.Append("            <dd>").Append(FormatDateTime(summary.DepartureDateTime)).AppendLine("</dd>");
        html.Append(
            """
                      </div>
                      <div>
                        <dt>Travelers</dt>
            """);
        html.Append("            <dd>").Append(FormatTravelers(summary.NumberOfAdults, summary.NumberOfChildren))
            .AppendLine("</dd>");
        html.Append(
            """
                      </div>
                      <div>
                        <dt>Planned days</dt>
            """);
        html.Append("            <dd>").Append(days.Count.ToString(DisplayCulture)).AppendLine("</dd>");
        html.Append(
            """
                      </div>
                    </dl>
                  </article>

                  <article class="card budget">
                    <p class="section-label">Budget</p>
                    <h2>Estimated spend</h2>
            """);
        html.Append("        <p class=\"budget-total\">").Append(FormatCurrency(summary.TotalEstimatedCostUsd))
            .AppendLine("</p>");
        html.Append("        <p class=\"muted\">of ").Append(FormatCurrency(summary.BudgetUsd)).AppendLine(" total budget</p>");
        html.Append("        <div class=\"progress\" role=\"progressbar\" aria-label=\"Budget used\" aria-valuemin=\"0\" aria-valuemax=\"100\" aria-valuenow=\"")
            .Append(decimal.Round(budgetUsage, 0).ToString(CultureInfo.InvariantCulture))
            .Append("\" style=\"--progress: ")
            .Append(budgetUsage.ToString("0.##", CultureInfo.InvariantCulture))
            .AppendLine("%\"><span></span></div>");
        html.Append("        <div class=\"budget-row\"><span>Remaining</span><strong>")
            .Append(FormatCurrency(summary.RemainingBudgetUsd)).AppendLine("</strong></div>");
        html.Append(
            """
                  </article>

                  <article class="card accommodation">
                    <p class="section-label">Stay</p>
                    <div class="accommodation-layout">
                      <div>
            """);
        html.Append("            <h2>").Append(Encode(accommodation.Name)).AppendLine("</h2>");
        html.Append("            <p class=\"muted\">").Append(Encode(accommodation.Type)).Append(" in ")
            .Append(Encode(accommodation.District)).AppendLine("</p>");
        html.Append("            <p class=\"price\">").Append(FormatCurrency(accommodation.TotalStayPriceUsd))
            .AppendLine(" <span class=\"muted\">total stay</span></p>");
        AppendMapLink(html, accommodation.Latitude, accommodation.Longitude);
        AppendStringList(html, accommodation.KeyAmenities, "chips", "chip");
        html.Append(
            """
                      </div>
                      <div>
                        <h3>Why this stay was selected</h3>
            """);
        if (accommodation.WhySelected is { Count: > 0 })
        {
            AppendStringList(html, accommodation.WhySelected, "reason-list");
        }
        else
        {
            html.AppendLine("        <p class=\"muted\">No selection rationale was provided.</p>");
        }

        html.Append(
            """
                      </div>
                    </div>
                  </article>
            """);

        if (warnings.Count > 0)
        {
            html.Append(
                """

                      <aside class="card warnings" aria-labelledby="warnings-heading">
                        <p class="section-label">Before you go</p>
                        <h2 id="warnings-heading">Important notes</h2>
                """);
            AppendStringList(html, warnings, "warning-list");
            html.AppendLine("      </aside>");
        }

        html.AppendLine("    </section>");
        html.AppendLine("    <section class=\"itinerary\" aria-labelledby=\"itinerary-heading\">");
        html.AppendLine("      <div class=\"itinerary-heading\">");
        html.AppendLine("        <p class=\"section-label\">Itinerary</p>");
        html.AppendLine("        <h2 id=\"itinerary-heading\">Day by day</h2>");
        html.AppendLine("      </div>");

        if (days.Count == 0)
        {
            html.AppendLine("      <p class=\"empty\">No daily itinerary has been scheduled yet.</p>");
        }
        else
        {
            for (var index = 0; index < days.Count; index++)
            {
                AppendDay(html, days[index], index + 1, open: index == 0);
            }
        }

        html.AppendLine("    </section>");
        html.Append(
            """

                <section class="card explanation">
                  <p class="section-label">Plan rationale</p>
                  <h2>How this itinerary comes together</h2>
            """);
        html.Append("      <p>")
            .Append(Encode(string.IsNullOrWhiteSpace(plan.OverallExplanation)
                ? "No additional plan explanation was provided."
                : plan.OverallExplanation))
            .AppendLine("</p>");
        html.Append(
            """
                </section>

                <p class="footer">Generated trip plan. Confirm opening hours, reservations, prices, and travel conditions before departure.</p>
              </main>

              <script>
                (() => {
                  const days = () => document.querySelectorAll("details.day");
                  document.querySelector('[data-action="expand"]').addEventListener("click", () => {
                    days().forEach(day => day.open = true);
                  });
                  document.querySelector('[data-action="collapse"]').addEventListener("click", () => {
                    days().forEach(day => day.open = false);
                  });
                  document.querySelector('[data-action="print"]').addEventListener("click", () => window.print());
                  window.addEventListener("beforeprint", () => days().forEach(day => day.open = true));
                })();
              </script>
            </body>
            </html>
            """);

        return html.ToString();
    }

    private static void AppendDay(StringBuilder html, TripDayPlan day, int dayNumber, bool open)
    {
        var items = day.Items ?? [];
        html.Append("      <details class=\"day\"");
        if (open)
        {
            html.Append(" open");
        }

        html.AppendLine(">");
        html.AppendLine("        <summary>");
        html.Append("          <span class=\"day-number\">").Append(dayNumber.ToString(DisplayCulture))
            .AppendLine("</span>");
        html.Append("          <span class=\"day-title\"><strong>").Append(Encode(day.DayTheme))
            .Append("</strong><span>").Append(FormatDayDate(day.Date)).Append(" &middot; ")
            .Append(items.Count.ToString(DisplayCulture)).Append(" item")
            .Append(items.Count == 1 ? string.Empty : "s").AppendLine("</span></span>");
        html.AppendLine("        </summary>");
        html.AppendLine("        <div class=\"day-content\">");
        html.Append("          <p class=\"day-summary\">")
            .Append(Encode(string.IsNullOrWhiteSpace(day.DaySummary)
                ? "No summary was provided for this day."
                : day.DaySummary))
            .AppendLine("</p>");

        if (items.Count == 0)
        {
            html.AppendLine("          <p class=\"empty\">No activities are scheduled for this day.</p>");
        }
        else
        {
            html.AppendLine("          <div class=\"timeline\">");
            foreach (var item in items)
            {
                AppendActivity(html, item);
            }

            html.AppendLine("          </div>");
        }

        html.AppendLine("        </div>");
        html.AppendLine("      </details>");
    }

    private static void AppendActivity(StringBuilder html, ScheduledPlanItem item)
    {
        var typeLabel = GetTypeLabel(item.Type);
        var typeClass = GetTypeClass(item.Type);

        html.Append("            <article class=\"activity type-").Append(typeClass).AppendLine("\">");
        html.AppendLine("              <span class=\"activity-dot\" aria-hidden=\"true\"></span>");
        html.AppendLine("              <div>");
        html.AppendLine("                <div class=\"activity-top\">");
        html.Append("                  <h3>").Append(Encode(item.Title)).AppendLine("</h3>");
        html.Append("                  <time class=\"activity-time\" datetime=\"")
            .Append(item.StartTime.ToString("O", CultureInfo.InvariantCulture)).Append("\">")
            .Append(FormatTime(item.StartTime)).Append(" - ").Append(FormatTime(item.EndTime))
            .AppendLine("</time>");
        html.AppendLine("                </div>");
        html.Append("                <p class=\"activity-description\">").Append(Encode(item.Description))
            .AppendLine("</p>");
        html.AppendLine("                <div class=\"activity-meta\">");
        html.Append("                  <span class=\"type-badge\">").Append(Encode(typeLabel)).AppendLine("</span>");
        html.Append("                  <span>").Append(FormatDuration(item.DurationHours)).AppendLine("</span>");

        if (!string.IsNullOrWhiteSpace(item.District))
        {
            html.Append("                  <span>").Append(Encode(item.District)).AppendLine("</span>");
        }

        html.Append("                  <span>")
            .Append(item.EstimatedCostUsd == 0 ? "No estimated cost" : FormatCurrency(item.EstimatedCostUsd))
            .AppendLine("</span>");
        html.AppendLine("                </div>");

        if (item.Latitude.HasValue && item.Longitude.HasValue)
        {
            AppendMapLink(html, item.Latitude.Value, item.Longitude.Value, indentation: "                ");
        }

        html.AppendLine("              </div>");
        html.AppendLine("            </article>");
    }

    private static void AppendStringList(
        StringBuilder html,
        IReadOnlyList<string>? values,
        string listClass,
        string? itemClass = null)
    {
        if (values is not { Count: > 0 })
        {
            return;
        }

        html.Append("        <ul class=\"").Append(listClass).AppendLine("\">");
        foreach (var value in values)
        {
            html.Append("          <li");
            if (itemClass is not null)
            {
                html.Append(" class=\"").Append(itemClass).Append('"');
            }

            html.Append('>').Append(Encode(value)).AppendLine("</li>");
        }

        html.AppendLine("        </ul>");
    }

    private static void AppendMapLink(
        StringBuilder html,
        double latitude,
        double longitude,
        string indentation = "            ")
    {
        var coordinates = string.Create(
            CultureInfo.InvariantCulture,
            $"{latitude:0.######},{longitude:0.######}");
        var mapUrl = $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(coordinates)}";

        html.Append(indentation).Append("<a class=\"map-link\" href=\"").Append(Encode(mapUrl))
            .AppendLine("\" target=\"_blank\" rel=\"noopener noreferrer\">View on map</a>");
    }

    private static string GetTypeLabel(PlanItemType type) => type switch
    {
        PlanItemType.AccommodationCheckIn => "Check-in",
        PlanItemType.AccommodationCheckOut => "Check-out",
        PlanItemType.FreeTime => "Free time",
        _ => type.ToString()
    };

    private static string GetTypeClass(PlanItemType type) => type switch
    {
        PlanItemType.AccommodationCheckIn => "accommodation-check-in",
        PlanItemType.AccommodationCheckOut => "accommodation-check-out",
        PlanItemType.FreeTime => "free-time",
        PlanItemType.Arrival => "arrival",
        PlanItemType.Departure => "departure",
        PlanItemType.Attraction => "attraction",
        PlanItemType.Restaurant => "restaurant",
        PlanItemType.Break => "break",
        PlanItemType.Transport => "transport",
        _ => "other"
    };

    private static string FormatTravelers(int adults, int children)
    {
        var travelers = $"{adults.ToString(DisplayCulture)} adult{(adults == 1 ? string.Empty : "s")}";
        return children > 0
            ? $"{travelers}, {children.ToString(DisplayCulture)} child{(children == 1 ? string.Empty : "ren")}"
            : travelers;
    }

    private static string FormatDuration(double durationHours)
    {
        var totalMinutes = Math.Max(0, (int)Math.Round(durationHours * 60));
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;

        return (hours, minutes) switch
        {
            ( > 0, > 0) => $"{hours} hr {minutes} min",
            ( > 0, _) => $"{hours} hr",
            _ => $"{minutes} min"
        };
    }

    private static string FormatCurrency(decimal value) => value.ToString("C0", DisplayCulture);
    private static string FormatDate(DateTime value) => Encode(value.ToString("MMM d, yyyy", DisplayCulture));
    private static string FormatDateTime(DateTime value) => Encode(value.ToString("MMM d, yyyy 'at' h:mm tt", DisplayCulture));
    private static string FormatDayDate(DateOnly value) => Encode(value.ToString("dddd, MMMM d", DisplayCulture));
    private static string FormatTime(DateTime value) => Encode(value.ToString("h:mm tt", DisplayCulture));
    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
