using System.Collections.Generic;

namespace AppGrooming.Models
{
    public class ReceptionistDashboardModel
    {
        public int PendingOrders { get; set; }
        public int InProgressOrders { get; set; }
        public int ReadyOrders { get; set; }
        public List<WorkOrderViewModel> WorkOrders { get; set; }
        public List<CustomerModel> Customers { get; set; }
        public List<UserModel> Groomers { get; set; }
        public List<ServiceModel> Services { get; set; }
    }
}
