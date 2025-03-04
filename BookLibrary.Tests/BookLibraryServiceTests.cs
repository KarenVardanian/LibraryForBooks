using LibraryForBooks.Entities;
using LibraryForBooks.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;
using System.Xml.Linq;

namespace LibraryForBooks.Tests
{
    [TestFixture]
    public class BookLibraryServiceTests
    {
        private Mock<ILogger<BookLibraryService>> _loggerMock;
        private BookLibraryService _service;
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<BookLibraryService>>();
            _service = new BookLibraryService(_loggerMock.Object);
            _testFilePath = "test_books.xml";
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Test]
        public void LoadFromXml_FileNotFound_ThrowsFileNotFoundException()
        {
            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() => _service.LoadFromXml("non_existing_file.xml"));
            Assert.That(ex.Message, Does.Contain("File not found"));
        }

        [Test]
        public void SaveToXml_SavesBooksToFile()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Book 1", Author = "Author 1", Pages = 100 },
                new Book { Title = "Book 2", Author = "Author 2", Pages = 150 }
            };
            foreach (var book in books)
            {
                _service.AddBook(book);
            }

            // Act
            _service.SaveToXml(_testFilePath);

            // Assert
            var loadedDocument = XDocument.Load(_testFilePath);
            var savedBooks = loadedDocument.Root.Elements("Book").ToList();
            Assert.That(savedBooks.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddBook_ValidBook_AddsBookSuccessfully()
        {
            // Arrange
            var book = new Book { Title = "New Book", Author = "New Author", Pages = 120 };

            // Act
            _service.AddBook(book);

            // Assert
            var sortedBooks = _service.GetSortedBooks().ToList();
            Assert.That(sortedBooks.Count, Is.EqualTo(1));
            Assert.That(sortedBooks[0].Title, Is.EqualTo("New Book"));
        }

        [Test]
        public void GetSortedBooks_SortsBooksByAuthorThenByTitle()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Test 2", Author = "Author 2", Pages = 150 },
                new Book { Title = "Test 1", Author = "Author 1", Pages = 100 },
                new Book { Title = "Test 3", Author = "Author 1", Pages = 120 }
            };

            foreach (var book in books)
            {
                _service.AddBook(book);
            }

            // Act
            var sortedBooks = _service.GetSortedBooks().ToList();

            // Assert
            Assert.That(sortedBooks[0].Author, Is.EqualTo("Author 1"));
            Assert.That(sortedBooks[0].Title, Is.EqualTo("Test 1"));
            Assert.That(sortedBooks[2].Author, Is.EqualTo("Author 2"));
        }

        [Test]
        public void SearchByTitle_EmptyTitle_ReturnsEmptyList()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Test 1", Author = "Author 1", Pages = 100 },
                new Book { Title = "Test 2", Author = "Author 2", Pages = 150 }
            };

            foreach (var book in books)
            {
                _service.AddBook(book);
            }

            // Act
            var result = _service.SearchByTitle("");

            // Assert
            Assert.IsEmpty(result);
        }
    }
}
