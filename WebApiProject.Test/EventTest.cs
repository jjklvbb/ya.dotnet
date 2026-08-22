using WebApiProject.Models;

namespace WebApiProject.Test
{
    public class EventTest
    {
        [Fact]
        public void Constructor_EmptyTitle_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Event(
                    Guid.NewGuid(),
                    "",
                    "Описание",
                    DateTime.Now.AddDays(1),
                    DateTime.Now.AddDays(2)));
        }

        [Fact]
        public void Constructor_EndAtBeforeStartAt_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Event(
                    Guid.NewGuid(),
                    "Новое название",
                    "Описание",
                    DateTime.Now.AddDays(2),
                    DateTime.Now.AddDays(1)));
        }
    }
}
