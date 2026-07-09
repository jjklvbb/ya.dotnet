using OrderManagement.Models;

namespace OrderManagement.Interfaces
{
    public interface IOrderRepository
    {
        Order? GetById(int id);
        List<Order> GetAll();
        void Add(Order order);
        void Update(Order order);
    }
}
