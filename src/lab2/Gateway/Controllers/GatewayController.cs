using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Gateway.Services;
using Gateway.DTO;
using Gateway.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("/api/v1/")]
    public class GatewayController : ControllerBase
    {
        private readonly ReservationConnect _reservationsConnect;
        private readonly PaymentConnect _paymentsConnect;
        private readonly LoyaltyConnect _loyaltyConnect;

        public GatewayController( ReservationConnect reservationsConnect,
            PaymentConnect paymentsConnect, LoyaltyConnect loyaltyConnect)
        {
            
            _reservationsConnect = reservationsConnect;
            _paymentsConnect = paymentsConnect;
            _loyaltyConnect = loyaltyConnect;
        }

        /// <summary>
        /// Проверяет жив ли ещё сервис
        /// </summary>
        /// <returns>Если сервис жив возвращает 200 </returns>
        /// <response code="200" cref="Person">Работает</response>
        [IgnoreAntiforgeryToken]
        [HttpGet("/manage/health")]
        public async Task<ActionResult> HealthCheck()
        {
            return Ok();
        }

        /// <summary>
        /// Получить информацию 
        /// </summary>
        /// <returns>Если сервис жив возвращает 200 </returns>
        /// <response code="200" cref="Person">Работает</response>
        [HttpGet("hotels")]
        public async Task<PaginationResponse<IEnumerable<Hotels>>?> GetAllHotels(
        [FromQuery] int? page,
        [FromQuery] int? size)
        {
            var response = await _reservationsConnect.GetHotelsAsync(page, size);
            return response;
        }

        /// <summary>
        /// Возвращает информацию о пользователе
        /// </summary>
        /// <returns>Запись о пользователе и его статус лояльности  </returns>
        /// <response code="200" cref="Person">Работает</response>
        [HttpGet("me")]
        public async Task<UserInfoResponse?> GetUserInfoByUsername(
        [FromHeader(Name = "X-User-Name")] string xUserName)
        {
            var reservations = await _reservationsConnect.GetReservationsByUsernameAsync(xUserName);
            if (reservations == null || !reservations.Any())
            {
                return null;
            }

            var response = new UserInfoResponse();
            response.Reservations = new List<UserReservationInfo>();
            var tasks = reservations.Select(reservation => Task.Run(async () =>
            {
                var hotel = await _reservationsConnect.GetHotelsByIdAsync(reservation.HotelId);
                var payment = await _paymentsConnect.GetPaymentByUidAsync(reservation.PaymentUid);

                response.Reservations.Add(new UserReservationInfo()
                {
                    ReservationUid = reservation.ReservationUid,
                    Status = reservation.Status,
                    StartDate = DateOnly.FromDateTime(reservation.StartDate),
                    EndDate = DateOnly.FromDateTime(reservation.EndDate),
                    Hotel = new HotelInfo()
                    {
                        HotelUid = hotel.HotelUid,
                        Name = hotel.Name,
                        FullAddress = hotel.Country + ", " + hotel.City + ", " + hotel.Address,
                        Stars = hotel.Stars,
                    },
                    Payment = new PaymentInfo()
                    {
                        Status = payment.Status,
                        Price = payment.Price,
                    },
                });
            }));

            await Task.WhenAll(tasks);

            var loyalty = await _loyaltyConnect.GetLoyaltyByUsernameAsync(xUserName);

            response.Loyalty = new LoyaltyInfo()
            {
                Status = loyalty.Status,
                Discount = loyalty.Discount,
            };

            return response;
        }

        /// <summary>
        /// Получить информацию о бронировании
        /// </summary>
        /// <returns>Записи о всех бронях с отелями и оплатами </returns>
        /// <response code="200" cref="Person">Работает</response>
        [HttpGet("reservations")]
        public async Task<List<UserReservationInfo>?> GetReservationsInfoByUsername(
        [FromHeader(Name = "X-User-Name")] string xUserName)
        {
            var reservations = await _reservationsConnect.GetReservationsByUsernameAsync(xUserName);
            if (reservations == null || !reservations.Any())
            {
                return null;
            }

            var response = new List<UserReservationInfo>();
            var tasks = reservations.Select(reservation => Task.Run(async () =>
            {
                var hotel = await _reservationsConnect.GetHotelsByIdAsync(reservation.HotelId);
                var payment = await _paymentsConnect.GetPaymentByUidAsync(reservation.PaymentUid);
                
                response.Add(new UserReservationInfo()
                {
                    ReservationUid = reservation.ReservationUid,
                    Status = reservation.Status,
                    StartDate = DateOnly.FromDateTime(reservation.StartDate),
                    EndDate = DateOnly.FromDateTime(reservation.EndDate),
                    Hotel = new HotelInfo()
                    {
                        HotelUid = hotel.HotelUid,
                        Name = hotel.Name,
                        FullAddress = hotel.Country + ", " + hotel.City + ", " + hotel.Address,
                        Stars = hotel.Stars,
                    },
                    Payment = new PaymentInfo()
                    {
                        Status = payment.Status,
                        Price = payment.Price,
                    },
                });
            }));

            
            await Task.WhenAll(tasks);

            return response;
        }

        [HttpGet("reservations/{reservationsUid}")]
        public async Task<ActionResult<UserReservationInfo>?> GetReservationsInfoByUsername(
        [FromRoute] Guid reservationsUid,
        [FromHeader(Name = "X-User-Name")] string xUserName)
        {
            var reservation = await _reservationsConnect.GetReservationsByUidAsync(reservationsUid);
            if (reservation == null)
            {
                return BadRequest();
            }

            if (!reservation.Username.Equals(xUserName))
            {
                return BadRequest();
            }

            var hotel = await _reservationsConnect.GetHotelsByIdAsync(reservation.HotelId);
            var payment = await _paymentsConnect.GetPaymentByUidAsync(reservation.PaymentUid);

            var response = new UserReservationInfo()
            {
                ReservationUid = reservation.ReservationUid,
                Status = reservation.Status,
                StartDate = DateOnly.FromDateTime(reservation.StartDate),
                EndDate = DateOnly.FromDateTime(reservation.EndDate),
                Hotel = new HotelInfo()
                {
                    HotelUid = hotel.HotelUid,
                    Name = hotel.Name,
                    FullAddress = hotel.Country + ", " + hotel.City + ", " + hotel.Address,
                    Stars = hotel.Stars,
                },
                Payment = new PaymentInfo()
                {
                    Status = payment.Status,
                    Price = payment.Price,
                },
            };

            
            return response;
        }

        [HttpPost("reservations")]
        public async Task<ActionResult<CreateReservationResponse?>> CreateReservation(
        [FromHeader(Name = "Name")] string Name,
        [FromBody] CreateReservationRequest request)
        {

            var hotel = await _reservationsConnect.GetHotelsByUidAsync(request.HotelUid);

            if (hotel == null)
            {
                return BadRequest(null);
            }

            int sum = ((request.EndDate - request.StartDate).Days) * hotel.Price;

            var loyalty = await _loyaltyConnect.GetLoyaltyByUsernameAsync(Name);

            
            if (loyalty == null)
            {
                sum -= sum * 5 / 100;
            }
            else
            {
                sum -= sum * loyalty.Discount / 100;
            }

            Payment paymentRequest = new Payment()
            {
                Price = sum,
            };

            var payment = await _paymentsConnect.CreatePaymentAsync(paymentRequest);

            if (payment == null)
            {
                return BadRequest(null);
            }

            loyalty = await _loyaltyConnect.PutLoyaltyByUsernameAsync(Name);

            Reservation reservationRequest = new Reservation()
            {
                PaymentUid = payment.PaymentUid,
                HotelId = hotel.Id,
                EndDate = request.EndDate,
                StartDate = request.StartDate,
            };

            var reservation = await _reservationsConnect.CreateReservationAsync(Name, reservationRequest);

            var reservationResponse = new CreateReservationResponse()
            {
                ReservationUid = reservation.ReservationUid,
                HotelUid = hotel.HotelUid,
                //StartDate = DateOnly.FromDateTime(reservation.StartDate),
                //EndDate = DateOnly.FromDateTime(reservation.EndDate),
                StartDate = DateOnly.FromDateTime(reservation.StartDate),
                EndDate = DateOnly.FromDateTime(reservation.EndDate),
                Status = reservation.Status,
                Discount = loyalty.Discount,
                Payment = new PaymentInfo()
                {
                    Status = payment.Status,
                    Price = payment.Price,
                }
            };

            return Ok(reservationResponse);
        }


        /// <summary>
        /// Получить данные по бронированию
        /// </summary>
        /// <returns> Запись о бронировании reservationsUid</returns>

        [HttpDelete("reservations/{reservationsUid}")]
        public async Task<ActionResult> DeleteReservationsByUid(
        [FromRoute] Guid reservationsUid,
        [FromHeader(Name = "Name")] string Name)
        {
            var reservation = await _reservationsConnect.GetReservationsByUidAsync(reservationsUid);
            if (reservation == null)
            {
                return BadRequest();
            }

            var updateReservationTask = _reservationsConnect.DeleteReservationAsync(reservation.ReservationUid);

            var updatePaymentTask = _paymentsConnect.DeletePaymentByUidAsync(reservation.PaymentUid);

            var updateLoyaltyTask = _loyaltyConnect.DeleteLoyaltyByUsernameAsync(Name);

            await updateReservationTask;
            await updatePaymentTask;
            await updateLoyaltyTask;

            return Ok(null);
        }


        /// <summary>
        /// Возвращает информацию о лояльности пользователя
        /// </summary>
        /// <returns> статус лояльности  </returns>
        
        [HttpGet("loyalty")]
        public async Task<LoyaltyInfoResponse?> GetLoyaltyInfoByUsername(
        [FromHeader(Name = "Name")] string Name)
        {
            var loyalty = await _loyaltyConnect.GetLoyaltyByUsernameAsync(Name);

            var response = new LoyaltyInfoResponse()
            {
                Status = loyalty.Status,
                Discount = loyalty.Discount,
                ReservationCount = loyalty.ReservationCount,
            };

            return response;
        }


    }
}
