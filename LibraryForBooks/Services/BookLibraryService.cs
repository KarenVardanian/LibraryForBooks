using LibraryForBooks.Constants;
using LibraryForBooks.Entities;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace LibraryForBooks.Services
{
    public class BookLibraryService
    {
        private readonly List<Book> bookList = new List<Book>();
        private readonly ILogger<BookLibraryService> _logger;

        public BookLibraryService(ILogger<BookLibraryService> logger)
        {
            _logger = logger;
        }

        public void LoadFromXml(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError(Errors.ErrorFileNotFound, filePath);
                throw new FileNotFoundException(string.Format(Errors.ErrorFileNotFound, filePath), filePath);
            }

            var doc = XDocument.Load(filePath);
            bookList.Clear();

            foreach (var xElement in doc.Root.Elements("Book"))
            {
                var title = xElement.Element("Title");
                var author = xElement.Element("Author");
                var pages = (int)xElement.Element("Pages");

                var book = new Book
                {
                    Title = title.ToString(),
                    Author = author.ToString(),
                    Pages = pages
                };

                bookList.Add(book);
            }
        }

        public void SaveToXml(string path)
        {
            var document = new XDocument(
                new XElement("Books",
                    bookList.Select(b => new XElement("Book",
                        new XElement("Title", b.Title),
                        new XElement("Author", b.Author),
                        new XElement("Pages", b.Pages)
                    ))
                )
            );
            document.Save(path);
        }

        public void AddBook(Book book)
        {
            if (book == null)
            {
                _logger.LogError(Errors.ErrorBookNull);
                throw new ArgumentNullException(nameof(book), Errors.ErrorBookNull);
            }

            bookList.Add(book);
        }

        public IEnumerable<Book> GetSortedBooks()
        {
            var result = bookList.OrderBy(b => b.Author).ThenBy(b => b.Title);
            return result;
        }

        public IEnumerable<Book> SearchByTitle(string titlePart)
        {
            if (string.IsNullOrWhiteSpace(titlePart))
            {
                _logger.LogWarning(Errors.ErrorEmptyTitleSearch);
                return Enumerable.Empty<Book>();
            }

            var result = bookList.Where(b => b.Title.Contains(titlePart, StringComparison.OrdinalIgnoreCase));
            return result;
        }
    }
}
