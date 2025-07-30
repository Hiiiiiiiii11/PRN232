using BussinessObjects;
using Repositories.IRepository;
using Repositories.IRepository.Appointments;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Users;
using Services.DTOs.Appointment;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class AppointmentService : IAppointmentService
    {
        private IAppointmentRepository _apointmentRepository;
        private IUserRepository _userRepository;
        private IConsultantRepository _consultantRepository;
        private IConsultantCalendarService _calendarService;
        private IUnitOfWork _unitOfWork;
        
        public AppointmentService(
            IAppointmentRepository appointmentRepository, 
            IUnitOfWork unitOfWork, 
            IUserRepository userRepository, 
            IConsultantRepository consultantRepository,
            IConsultantCalendarService calendarService)
        {
            _apointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _consultantRepository = consultantRepository;
            _calendarService = calendarService;
        }
        public async Task<ApppointmentResponse> CreateAppointment(AppointmentRequest request, int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            // Check if the requested slot is available if consultant is specified
            if (request.ConsultantID.HasValue)
            {
                var isSlotAvailable = await _calendarService.IsSlotAvailableAsync(request.ConsultantID.Value, request.ScheduledAt);
                if (!isSlotAvailable)
                {
                    throw new InvalidOperationException("Thời gian đã chọn không có sẵn. Vui lòng chọn thời gian khác.");
                }
            }

            var appointment = new Appointment();
            appointment.User = user;
            if (request.ConsultantID != null)
            {
                var consultant = await _consultantRepository.GetByIdAsync(request.ConsultantID);
                appointment.Consultant = consultant;
            }
            appointment.ScheduledAt = request.ScheduledAt;
            appointment.Status = "Pending";
            appointment.Notes = request.Notes;
            appointment.CreatedAt = DateTime.Now;
            
            _apointmentRepository.Insert(appointment);
            await _unitOfWork.SaveAsync();

            // Book the slot if consultant is specified
            if (request.ConsultantID.HasValue)
            {
                await _calendarService.BookSlotAsync(request.ConsultantID.Value, request.ScheduledAt, appointment.AppointmentID);
            }

            var appointmentResponse = new ApppointmentResponse();
            appointmentResponse.AppointmentID = appointment.AppointmentID;
            appointmentResponse.ConsultantID = appointment.ConsultantID;
            appointmentResponse.ScheduledAt = appointment.ScheduledAt;
            appointmentResponse.Status = appointment.Status;
            appointmentResponse.Notes = appointment.Notes;
            appointmentResponse.CreatedAt = appointment.CreatedAt;
            return appointmentResponse;
        }

        public async Task<List<ApppointmentResponse>> GetAllAppointment()
        {
            var appointments = await _apointmentRepository.GetAllAsync();
            List<ApppointmentResponse> apppointmentResponses = new List<ApppointmentResponse>();
            foreach (var appointment in appointments)
            {
                var appointmentResponse = new ApppointmentResponse();
                appointmentResponse.AppointmentID = appointment.AppointmentID;
                appointmentResponse.ConsultantID = appointment.ConsultantID;
                appointmentResponse.ScheduledAt = appointment.ScheduledAt;
                appointmentResponse.Status = appointment.Status;
                appointmentResponse.Notes = appointment.Notes;
                appointmentResponse.CreatedAt = appointment.CreatedAt;
                apppointmentResponses.Add(appointmentResponse);
            }
            return apppointmentResponses;
        }
    }
}
