# Ramsey County Workflow

## Beacon Scraper Methodology
Automates the Ramsey County Beacon GIS portal (Playwright MCP + Postman)
to pull property records at scale rather than manual lookup. Output is a
structured record set: parcel ID, owner name/address, tax status, and
assessed value.

## Ghost / Vacant Property Qualification
A property is flagged as a preservation candidate when 2 or more of the
following signals are present:
- Tax delinquency (nonzero delinquent balance)
- Owner mailing address differs from the property address
- Ownership is held by an estate or trust

A single signal alone is not sufficient — see Guardrail 2 in SKILL.md.

## Field Inspection Handoff
Once a candidate property is qualified, it moves to the mobile field
inspection app (Blazor WASM PWA) for a walkthrough. Inspection output
(photos + notes) feeds `inspection_report_builder.py`.
