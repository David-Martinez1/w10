using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Gateway.DTO;
using Gateway.Utils;
using Gateway.Models;
using System.Net;

namespace Gateway.ServiceInterfaces
{
    public interface IReservationService
    {
        public Task<bool> HealthCheckAsync();

        public Task<PaginationResponse<IEnumerable<Hotels>>?> GetHotelsAsync(int? page,
        int? size);

        public Task<Hotels?> GetHotelsByIdAsync(int? id);

        public Task<Hotels?> GetHotelsByUidAsync(Guid? id);


        public Task<IEnumerable<Reservation>?> GetReservationsByUsernameAsync(string username);

        public Task<Reservation?> GetReservationsByUidAsync(Guid reservationUid);

        public Task<Reservation?> CreateReservationAsync(string username, Reservation request);



        public Task<Reservation?> DeleteReservationAsync(Guid reservationUid);
    }
}
