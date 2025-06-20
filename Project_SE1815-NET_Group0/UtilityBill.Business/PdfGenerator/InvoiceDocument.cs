// File: UtilityBill.Business/PdfGenerator/InvoiceDocument.cs

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure; // Đảm bảo có using này
using UtilityBill.Data.Models;
using System;

// KHÔNG CẦN 'using System.ComponentModel;'

namespace UtilityBill.Business.PdfGenerator
{
    public class InvoiceDocument : IDocument
    {
        private readonly Invoice _invoice;
        private readonly User _tenant;

        public InvoiceDocument(Invoice invoice, User tenant)
        {
            _invoice = invoice;
            _tenant = tenant;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        // Sửa ở đây: Chỉ định rõ IDocumentContainer
        public void Compose(QuestPDF.Infrastructure.IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    // Sử dụng font có sẵn của QuestPDF để tránh lỗi font trên các máy khác nhau
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Calibri));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
        }

        // Sửa ở đây: Chỉ định rõ IContainer
        void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
        {
            // Bọc tất cả nội dung trong một Column cha duy nhất
            container.Column(column =>
            {
                // Item 1: Row chứa thông tin khu trọ
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("KHU TRỌ AN BÌNH").Bold().FontSize(16);
                        col.Item().Text("Địa chỉ: 123 Đường ABC, Phường XYZ, Quận 1, TP.HCM");
                        col.Item().Text("Điện thoại: 0987 654 321");
                        col.Item().Text("Email: khutroanbinh@email.com");
                    });

                    row.ConstantItem(100).Height(50).Placeholder(); // Khoảng trống
                });

                // Item 2: Column chứa tiêu đề hóa đơn
                column.Item().PaddingVertical(20).Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    col.Item().AlignCenter().Text("HÓA ĐƠN TIỀN NHÀ").SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);
                    col.Item().Text($"Kỳ: Tháng {_invoice.InvoicePeriodMonth}/{_invoice.InvoicePeriodYear}").AlignCenter();
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });
            });
        }

        // Sửa ở đây: Chỉ định rõ IContainer
        void ComposeContent(QuestPDF.Infrastructure.IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Thông tin khách hàng và hóa đơn
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("THÔNG TIN KHÁCH HÀNG").SemiBold();
                        col.Item().Text($"Người thuê: {_tenant.FullName}");
                        col.Item().Text($"Phòng: {_invoice.Room.RoomNumber}");
                    });
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("THÔNG TIN HÓA ĐƠN").SemiBold();
                        col.Item().Text($"Số HĐ: {_invoice.Id.ToString().Substring(0, 8).ToUpper()}");
                        col.Item().Text($"Ngày phát hành: {_invoice.CreatedAt:dd/MM/yyyy}");
                        col.Item().Text($"Hạn thanh toán: {_invoice.DueDate:dd/MM/yyyy}").Bold();
                    });
                });

                // Bảng chi tiết
                column.Item().PaddingTop(25).Element(ComposeTable);

                // Tổng tiền
                column.Item().AlignRight().PaddingTop(10).Row(row =>
                {
                    row.ConstantItem(100);
                    row.RelativeItem().Text("Tổng cộng thanh toán:").SemiBold().FontSize(14);
                    row.RelativeItem(0.5f).AlignRight().Text($"{_invoice.TotalAmount:N0} VNĐ").Bold().FontSize(14);
                });

                // Ghi chú
                column.Item().PaddingTop(40).Text("Ghi chú: Vui lòng thanh toán trước hạn chót. Mọi thắc mắc xin liên hệ ban quản lý. Xin cảm ơn!").Italic();
            });
        }

        // Sửa ở đây: Chỉ định rõ IContainer
        void ComposeTable(QuestPDF.Infrastructure.IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("STT");
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Nội dung");
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Số lượng");
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Đơn giá");
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Thành tiền");
                });

                int index = 1;
                foreach (var item in _invoice.InvoiceDetails)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(index.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Description);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.Quantity:N0}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.UnitPrice:N0}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{item.Amount:N0}");
                    index++;
                }
            });
        }
    }
}