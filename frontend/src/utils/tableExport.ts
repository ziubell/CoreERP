import type { utils as XLSXUtils, writeFile as XLSXWriteFile } from 'xlsx'

interface ExportColumn {
  key: string
  title: string
}

function prepareData(data: Record<string, any>[], columns: ExportColumn[]): string[][] {
  const headers = columns.map(c => c.title)
  const rows = data.map(row => columns.map(c => {
    const val = row[c.key]
    return val !== null && val !== undefined ? String(val) : ''
  }))
  return [headers, ...rows]
}

function prepareRows(data: Record<string, any>[], columns: ExportColumn[]): Record<string, any>[] {
  return data.map(row => {
    const obj: Record<string, any> = {}
    for (const c of columns)
      obj[c.title] = row[c.key] !== null && row[c.key] !== undefined ? row[c.key] : ''
    return obj
  })
}

export function exportToCsv(data: Record<string, any>[], columns: ExportColumn[], filename: string) {
  const rows = prepareData(data, columns)
  const csvContent = rows
    .map(row => row.map(cell => `"${String(cell).replace(/"/g, '""')}"`).join(','))
    .join('\n')

  const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' })
  downloadBlob(blob, `${filename}.csv`)
}

export async function exportToExcel(data: Record<string, any>[], columns: ExportColumn[], filename: string) {
  const XLSX = await import('xlsx')
  const rows = prepareRows(data, columns)
  const ws = XLSX.utils.json_to_sheet(rows)
  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, 'Dati')

  // Auto-size columns
  const colWidths = columns.map((c, i) => {
    const maxLen = Math.max(
      c.title.length,
      ...rows.map(r => String(r[c.title] ?? '').length),
    )
    return { wch: Math.min(maxLen + 2, 40) }
  })
  ws['!cols'] = colWidths

  XLSX.writeFile(wb, `${filename}.xlsx`)
}

export async function exportToPdf(data: Record<string, any>[], columns: ExportColumn[], filename: string) {
  const { default: jsPDF } = await import('jspdf')
  await import('jspdf-autotable')

  const doc = new jsPDF({ orientation: 'landscape' })
  const headers = columns.map(c => c.title)
  const rows = data.map(row => columns.map(c => {
    const val = row[c.key]
    return val !== null && val !== undefined ? String(val) : ''
  }))

  ;(doc as any).autoTable({
    head: [headers],
    body: rows,
    styles: { fontSize: 8 },
    headStyles: { fillColor: [105, 108, 255] },
    margin: { top: 15 },
    didDrawPage: (data: any) => {
      doc.setFontSize(12)
      doc.text(filename, 14, 10)
    },
  })

  doc.save(`${filename}.pdf`)
}

export function printTable(data: Record<string, any>[], columns: ExportColumn[], title: string) {
  const rows = prepareData(data, columns)
  const headers = rows[0]
  const bodyRows = rows.slice(1)

  const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <title>${title}</title>
      <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        h1 { font-size: 18px; margin-bottom: 10px; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; font-size: 12px; }
        th { background-color: #696cff; color: white; }
        tr:nth-child(even) { background-color: #f9f9f9; }
        @media print { body { margin: 0; } }
      </style>
    </head>
    <body>
      <h1>${title}</h1>
      <table>
        <thead><tr>${headers.map(h => `<th>${h}</th>`).join('')}</tr></thead>
        <tbody>${bodyRows.map(r => `<tr>${r.map(c => `<td>${c}</td>`).join('')}</tr>`).join('')}</tbody>
      </table>
    </body>
    </html>
  `
  const printWindow = window.open('', '_blank')
  if (printWindow) {
    printWindow.document.write(html)
    printWindow.document.close()
    printWindow.onload = () => {
      printWindow.print()
      printWindow.close()
    }
  }
}

export function copyTable(data: Record<string, any>[], columns: ExportColumn[]) {
  const rows = prepareData(data, columns)
  const text = rows.map(r => r.join('\t')).join('\n')
  navigator.clipboard.writeText(text)
}

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  link.click()
  URL.revokeObjectURL(url)
}
