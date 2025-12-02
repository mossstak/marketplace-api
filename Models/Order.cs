using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceApi.Models
{
    public class Order
    {
        public int Id {get; set;}
        public string BuyerId {get; set;} = "";
        public User? Buyer {get; set;}
        public decimal TotalAmount {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
        public ICollection<OrderItem> Items {get; set;} = new List<OrderItem>();
        public OrderStatus Status {get; set;} = OrderStatus.Pending;



    }
}