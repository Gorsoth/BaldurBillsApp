using Xunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BaldurBillsApp.Controllers;
using BaldurBillsApp.Models;
using BaldurBillsApp.Services;
using Microsoft.EntityFrameworkCore;
using BaldurBillsApp.ViewModels;


namespace BaldurBills.Tests.ControllersTests
{
    public class InvoicesListControllerTests
    {
        private readonly Mock<BaldurBillsDbContext> _mockContext;
        private readonly Mock<ISharedDataService> _mockSharedDataService;
        private readonly Mock<IWebHostEnvironment> _mockHostingEnvironment;
        private readonly Mock<PdfService> _mockPdfService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly InvoicesListController _controller;


        public InvoicesListControllerTests()
        {
            _mockContext = new Mock<BaldurBillsDbContext>();
            _mockSharedDataService = new Mock<ISharedDataService>();
            _mockHostingEnvironment = new Mock<IWebHostEnvironment>();
            _mockPdfService = new Mock<PdfService>();
            _mockMapper = new Mock<IMapper>();

            _controller = new InvoicesListController(
                _mockContext.Object,
                _mockSharedDataService.Object,
                _mockHostingEnvironment.Object,
                _mockPdfService.Object,
                _mockMapper.Object
            );
        }
        [Fact]
        public void UploadFile_Returns_ViewResult()
        {
            // Arrange
            _mockHostingEnvironment.Setup(h => h.WebRootPath).Returns("wwwroot");

            // Act
            var result = _controller.UploadFile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public async Task Index_WithNoSearchTerm_ReturnsAllInvoices()
        {
            // Arrange
            var mockInvoices = new List<InvoicesList>
        {
            new InvoicesList { InvoiceId = 1, InvoiceNumber = "INV001" },
            new InvoicesList { InvoiceId = 2, InvoiceNumber = "INV002" }
        }.AsQueryable();

            var mockSet = new Mock<DbSet<InvoicesList>>();
            mockSet.As<IQueryable<InvoicesList>>().Setup(m => m.Provider).Returns(mockInvoices.Provider);
            mockSet.As<IQueryable<InvoicesList>>().Setup(m => m.Expression).Returns(mockInvoices.Expression);
            mockSet.As<IQueryable<InvoicesList>>().Setup(m => m.ElementType).Returns(mockInvoices.ElementType);
            mockSet.As<IQueryable<InvoicesList>>().Setup(m => m.GetEnumerator()).Returns(mockInvoices.GetEnumerator());

            _mockContext.Setup(c => c.InvoicesLists).Returns(mockSet.Object);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<InvoicesList>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public void CalculateRemainingAmount_ReturnsCorrectValue()
        {
            // Arrange
            decimal? invoiceAmount = 1000m;
            var settlements = new List<Settlement>
        {
            new Settlement { SettlementAmount = 200m },
            new Settlement { SettlementAmount = 300m }
        };

            // Act
            var result = _controller.CalculateRemainingAmount(invoiceAmount, settlements);

            // Assert
            Assert.Equal(500m, result);
        }

        [Fact]
        public void Settlement_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int invalidId = 0;

            var mockInvoices = new List<InvoicesList>().AsQueryable();

            var mockSet = new Mock<DbSet<InvoicesList>>();
            mockSet.As<IQueryable<InvoicesList>>().Setup(m => m.Provider).Returns(mockInvoices.Provider);
            mockSet.As<IQueryable<InvoicesList>>().Setup(m => m.Expression).Returns(mockInvoices.Expression);
            mockSet.As<IQueryable<InvoicesList>>().Setup(m => m.ElementType).Returns(mockInvoices.ElementType);
            mockSet.As<IQueryable<InvoicesList>>().Setup(m => m.GetEnumerator()).Returns(mockInvoices.GetEnumerator());

            _mockContext.Setup(c => c.InvoicesLists).Returns(mockSet.Object);

            // Act
            var result = _controller.Settlement(invalidId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void AddSettlement_WithValidInvoiceId_ReturnsViewWithCorrectViewModel()
        {
            // Arrange
            int invoiceId = 1;
            var invoices = new List<InvoicesList>
             {
            new InvoicesList
                {
                    InvoiceId = invoiceId,
                    GrossAmount = 1000m,
                    NetAmount = 800m,
                    Currency = "USD",
                    InvoiceDate = new DateOnly(2023, 1, 1),
                    DueDate = new DateOnly(2023, 1, 31),
                    IsPaid = false,
                    VendorId = 1,
                    Vendor = new Vendor { VendorId = 1, VendorName = "Test Vendor" },
                    Rate = new ToPlnRate { RateId = 1, RateValue = 4.5m },
                    Attachments = new List<Attachment>
                        {
                            new Attachment { AttachmentId = 1, FilePath = "test.pdf" }
                        }
                }
                }.AsQueryable();

                        var settlements = new List<Settlement>
                    {
                        new Settlement { InvoiceId = invoiceId, SettlementAmount = 200m }
                    }.AsQueryable();
            //Mockowanie danych - po polsku
            var mockSetInvoices = new Mock<DbSet<InvoicesList>>();
            mockSetInvoices.As<IQueryable<InvoicesList>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockSetInvoices.As<IQueryable<InvoicesList>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockSetInvoices.As<IQueryable<InvoicesList>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockSetInvoices.As<IQueryable<InvoicesList>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            
            var mockSetSettlements = new Mock<DbSet<Settlement>>();
            mockSetSettlements.As<IQueryable<Settlement>>().Setup(m => m.Provider).Returns(settlements.Provider);
            mockSetSettlements.As<IQueryable<Settlement>>().Setup(m => m.Expression).Returns(settlements.Expression);
            mockSetSettlements.As<IQueryable<Settlement>>().Setup(m => m.ElementType).Returns(settlements.ElementType);
            mockSetSettlements.As<IQueryable<Settlement>>().Setup(m => m.GetEnumerator()).Returns(settlements.GetEnumerator());

            _mockContext.Setup(c => c.InvoicesLists).Returns(mockSetInvoices.Object);
            _mockContext.Setup(c => c.Settlements).Returns(mockSetSettlements.Object);

            // Act
            var result = _controller.AddSettlement(invoiceId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddSettlementViewModel>(viewResult.Model);
            Assert.Equal(invoiceId, model.InvoiceID);
            Assert.Equal(800m, model.RemainingAmount); // GrossAmount - SettlementAmount = 1000 - 200 = 800

            // Verify that the related entities are properly included
            var invoice = invoices.FirstOrDefault(i => i.InvoiceId == invoiceId);
            Assert.NotNull(invoice);
            Assert.Equal("Test Vendor", invoice.Vendor.VendorName);
         }


        [Fact]
        public void AddSettlement_WithInvalidInvoiceId_ReturnsViewWithDefaultViewModel()
        {
            // Arrange
            int invoiceId = 1;

            _mockContext.Setup(c => c.InvoicesLists.FirstOrDefault(It.IsAny<Func<InvoicesList, bool>>())).Returns((InvoicesList)null);

            // Act
            var result = _controller.AddSettlement(invoiceId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddSettlementViewModel>(viewResult.Model);
            Assert.Equal(invoiceId, model.InvoiceID);
            Assert.Equal(0m, model.RemainingAmount);
        }

    }
}
