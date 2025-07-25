using BussinessObjects;
using Services.DTOs.Appointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IAppointmentService
    {
        Task<ApppointmentResponse> CreateAppointment(AppointmentRequest request, int id);
        Task<List<ApppointmentResponse>> GetAllAppointment();
    }
}
