using System.Net.Http;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net;
using Gateway.Models;


namespace Gateway.ServiceInterfaces
{
    public interface IPaymentService
    {

        public Task<Payment?> RollBackPayment(Guid paymentUid);

        public Task<Payment?> CreatePaymentAsync(Payment request);
        public Task<bool> HealthCheckAsync();

        public Task<Payment?> GetPaymentByUidAsync(Guid paymentUid);

        public Task<Payment?> CancelPaymentByUidAsync(Guid paymentUid);

    }
}
