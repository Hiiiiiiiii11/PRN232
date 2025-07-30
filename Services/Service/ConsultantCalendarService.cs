using BussinessObjects;
using Repositories.IRepository;
using Repositories.IRepository.Consultants;
using Services.DTOs.Consultant;
using Services.IService;

namespace Services.Service
{
    public class ConsultantCalendarService : IConsultantCalendarService
    {
        private readonly IConsultantScheduleRepository _scheduleRepository;
        private readonly IConsultantAvailabilityRepository _availabilityRepository;
        private readonly IAppointmentSlotRepository _slotRepository;
        private readonly IConsultantRepository _consultantRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConsultantCalendarService(
            IConsultantScheduleRepository scheduleRepository,
            IConsultantAvailabilityRepository availabilityRepository,
            IAppointmentSlotRepository slotRepository,
            IConsultantRepository consultantRepository,
            IUnitOfWork unitOfWork)
        {
            _scheduleRepository = scheduleRepository;
            _availabilityRepository = availabilityRepository;
            _slotRepository = slotRepository;
            _consultantRepository = consultantRepository;
            _unitOfWork = unitOfWork;
        }

        #region Schedule Management

        public async Task<List<ConsultantScheduleDto>> GetConsultantScheduleAsync(int consultantId)
        {
            var schedules = await _scheduleRepository.GetActiveSchedulesByConsultantIdAsync(consultantId);
            return schedules.Select(s => new ConsultantScheduleDto
            {
                ScheduleID = s.ScheduleID,
                ConsultantID = s.ConsultantID,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDurationMinutes = s.SlotDurationMinutes,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<ConsultantScheduleDto> CreateScheduleAsync(int consultantId, CreateConsultantScheduleDto createDto)
        {
            // Check for conflicts
            var hasConflict = await _scheduleRepository.HasScheduleConflictAsync(
                consultantId, createDto.DayOfWeek, createDto.StartTime, createDto.EndTime);

            if (hasConflict)
                throw new InvalidOperationException("Lịch trình bị trung lặp với lịch đã có.");

            var schedule = new ConsultantSchedule
            {
                ConsultantID = consultantId,
                DayOfWeek = createDto.DayOfWeek,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                SlotDurationMinutes = createDto.SlotDurationMinutes,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _scheduleRepository.Insert(schedule);
            await _unitOfWork.SaveAsync();

            return new ConsultantScheduleDto
            {
                ScheduleID = schedule.ScheduleID,
                ConsultantID = schedule.ConsultantID,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                SlotDurationMinutes = schedule.SlotDurationMinutes,
                IsActive = schedule.IsActive
            };
        }

        public async Task<ConsultantScheduleDto> UpdateScheduleAsync(int consultantId, UpdateConsultantScheduleDto updateDto)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(updateDto.ScheduleID);
            if (schedule == null || schedule.ConsultantID != consultantId)
                throw new KeyNotFoundException("Không tìm thấy lịch trình.");

            // Check for conflicts excluding current schedule
            var hasConflict = await _scheduleRepository.HasScheduleConflictAsync(
                consultantId, schedule.DayOfWeek, updateDto.StartTime, updateDto.EndTime, schedule.ScheduleID);

            if (hasConflict)
                throw new InvalidOperationException("Lịch trình bị trung lặp với lịch đã có.");

            schedule.StartTime = updateDto.StartTime;
            schedule.EndTime = updateDto.EndTime;
            schedule.SlotDurationMinutes = updateDto.SlotDurationMinutes;
            schedule.IsActive = updateDto.IsActive;
            schedule.UpdatedAt = DateTime.Now;

            _scheduleRepository.Update(schedule);
            await _unitOfWork.SaveAsync();

            return new ConsultantScheduleDto
            {
                ScheduleID = schedule.ScheduleID,
                ConsultantID = schedule.ConsultantID,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                SlotDurationMinutes = schedule.SlotDurationMinutes,
                IsActive = schedule.IsActive
            };
        }

        public async Task<bool> DeleteScheduleAsync(int consultantId, int scheduleId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null || schedule.ConsultantID != consultantId)
                return false;

            _scheduleRepository.Delete(schedule);
            await _unitOfWork.SaveAsync();
            return true;
        }

        #endregion

        #region Availability Management

        public async Task<List<ConsultantAvailabilityDto>> GetConsultantAvailabilityAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            var availabilities = await _availabilityRepository.GetAvailabilitiesByDateRangeAsync(consultantId, startDate, endDate);
            return availabilities.Select(a => new ConsultantAvailabilityDto
            {
                AvailabilityID = a.AvailabilityID,
                ConsultantID = a.ConsultantID,
                Date = a.Date,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Notes = a.Notes
            }).ToList();
        }

        public async Task<ConsultantAvailabilityDto> SetAvailabilityAsync(int consultantId, CreateConsultantAvailabilityDto createDto)
        {
            var availability = new ConsultantAvailability
            {
                ConsultantID = consultantId,
                Date = createDto.Date.Date,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                Status = createDto.Status,
                Notes = createDto.Notes,
                CreatedAt = DateTime.Now
            };

            _availabilityRepository.Insert(availability);
            await _unitOfWork.SaveAsync();

            return new ConsultantAvailabilityDto
            {
                AvailabilityID = availability.AvailabilityID,
                ConsultantID = availability.ConsultantID,
                Date = availability.Date,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                Status = availability.Status,
                Notes = availability.Notes
            };
        }

        public async Task<ConsultantAvailabilityDto> UpdateAvailabilityAsync(int consultantId, UpdateConsultantAvailabilityDto updateDto)
        {
            var availability = await _availabilityRepository.GetByIdAsync(updateDto.AvailabilityID);
            if (availability == null || availability.ConsultantID != consultantId)
                throw new KeyNotFoundException("Không tìm thấy thông tin khả năng.");

            availability.StartTime = updateDto.StartTime;
            availability.EndTime = updateDto.EndTime;
            availability.Status = updateDto.Status;
            availability.Notes = updateDto.Notes;
            availability.UpdatedAt = DateTime.Now;

            _availabilityRepository.Update(availability);
            await _unitOfWork.SaveAsync();

            return new ConsultantAvailabilityDto
            {
                AvailabilityID = availability.AvailabilityID,
                ConsultantID = availability.ConsultantID,
                Date = availability.Date,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                Status = availability.Status,
                Notes = availability.Notes
            };
        }

        public async Task<bool> DeleteAvailabilityAsync(int consultantId, int availabilityId)
        {
            var availability = await _availabilityRepository.GetByIdAsync(availabilityId);
            if (availability == null || availability.ConsultantID != consultantId)
                return false;

            _availabilityRepository.Delete(availability);
            await _unitOfWork.SaveAsync();
            return true;
        }

        #endregion

        #region Calendar Views

        public async Task<WeeklyCalendarDto> GetWeeklyCalendarAsync(int consultantId, DateTime weekStartDate)
        {
            var weekStart = weekStartDate.Date;
            var weekEnd = weekStart.AddDays(6);

            var schedules = await GetConsultantScheduleAsync(consultantId);
            var dailyCalendars = new List<ConsultantCalendarDto>();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = weekStart.AddDays(i);
                var dailyCalendar = await GetDailyCalendarAsync(consultantId, currentDate);
                dailyCalendars.Add(dailyCalendar);
            }

            return new WeeklyCalendarDto
            {
                ConsultantID = consultantId,
                WeekStartDate = weekStart,
                DailyCalendars = dailyCalendars,
                WeeklySchedule = schedules
            };
        }

        public async Task<ConsultantCalendarDto> GetDailyCalendarAsync(int consultantId, DateTime date)
        {
            var availableSlots = await _slotRepository.GetAvailableSlotsByConsultantAndDateAsync(consultantId, date);
            var bookedSlots = await _slotRepository.GetSlotsByDateRangeAsync(consultantId, date.Date, date.Date.AddDays(1).AddTicks(-1));

            var availableSlotDtos = availableSlots.Select(slot => new AppointmentSlotDto
            {
                SlotID = slot.SlotID,
                ConsultantID = slot.ConsultantID,
                SlotDateTime = slot.SlotDateTime,
                DurationMinutes = slot.DurationMinutes,
                Status = slot.Status,
                AppointmentID = slot.AppointmentID
            }).ToList();

            var bookedSlotDtos = bookedSlots.Where(slot => slot.Status == "Booked").Select(slot => new AppointmentSlotDto
            {
                SlotID = slot.SlotID,
                ConsultantID = slot.ConsultantID,
                SlotDateTime = slot.SlotDateTime,
                DurationMinutes = slot.DurationMinutes,
                Status = slot.Status,
                AppointmentID = slot.AppointmentID
            }).ToList();

            return new ConsultantCalendarDto
            {
                ConsultantID = consultantId,
                Date = date.Date,
                AvailableSlots = availableSlotDtos,
                BookedSlots = bookedSlotDtos,
                HasAvailability = availableSlotDtos.Any()
            };
        }

        public async Task<List<AppointmentSlotDto>> GetAvailableSlotsAsync(int consultantId, DateTime date)
        {
            var slots = await _slotRepository.GetAvailableSlotsByConsultantAndDateAsync(consultantId, date);
            return slots.Select(slot => new AppointmentSlotDto
            {
                SlotID = slot.SlotID,
                ConsultantID = slot.ConsultantID,
                SlotDateTime = slot.SlotDateTime,
                DurationMinutes = slot.DurationMinutes,
                Status = slot.Status,
                AppointmentID = slot.AppointmentID
            }).ToList();
        }

        #endregion

        #region Slot Management

        public async Task GenerateSlotsForPeriodAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            var currentDate = startDate.Date;
            while (currentDate <= endDate.Date)
            {
                await _slotRepository.GenerateSlotsForDateAsync(consultantId, currentDate);
                currentDate = currentDate.AddDays(1);
            }
        }

        public async Task<bool> BookSlotAsync(int consultantId, DateTime slotDateTime, int appointmentId)
        {
            var slot = await _slotRepository.GetSlotByDateTimeAsync(consultantId, slotDateTime);
            if (slot == null || slot.Status != "Available")
                return false;

            slot.Status = "Booked";
            slot.AppointmentID = appointmentId;
            slot.UpdatedAt = DateTime.Now;

            _slotRepository.Update(slot);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> ReleaseSlotAsync(int consultantId, DateTime slotDateTime)
        {
            var slot = await _slotRepository.GetSlotByDateTimeAsync(consultantId, slotDateTime);
            if (slot == null)
                return false;

            slot.Status = "Available";
            slot.AppointmentID = null;
            slot.UpdatedAt = DateTime.Now;

            _slotRepository.Update(slot);
            await _unitOfWork.SaveAsync();
            return true;
        }

        #endregion

        #region Utility Methods

        public async Task<bool> IsSlotAvailableAsync(int consultantId, DateTime slotDateTime)
        {
            return await _slotRepository.IsSlotAvailableAsync(consultantId, slotDateTime);
        }

        public async Task<List<DateTime>> GetNextAvailableSlotsAsync(int consultantId, int count = 10)
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(30); // Look ahead 30 days

            var slots = await _slotRepository.GetSlotsByDateRangeAsync(consultantId, today, endDate);
            
            return slots
                .Where(slot => slot.Status == "Available" && slot.SlotDateTime > DateTime.Now)
                .Take(count)
                .Select(slot => slot.SlotDateTime)
                .ToList();
        }

        #endregion
    }
}