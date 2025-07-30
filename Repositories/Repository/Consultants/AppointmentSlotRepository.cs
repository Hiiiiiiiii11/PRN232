using BussinessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepository.Consultants;

namespace Repositories.Repository.Consultants
{
    public class AppointmentSlotRepository : GenericRepository<AppointmentSlot>, IAppointmentSlotRepository
    {
        private readonly DrugUsePreventionDBContext _context;

        public AppointmentSlotRepository(DrugUsePreventionDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<AppointmentSlot>> GetSlotsByConsultantIdAsync(int consultantId)
        {
            return await _context.AppointmentSlots
                .Where(slot => slot.ConsultantID == consultantId)
                .OrderBy(slot => slot.SlotDateTime)
                .ToListAsync();
        }

        public async Task<List<AppointmentSlot>> GetAvailableSlotsByConsultantAndDateAsync(int consultantId, DateTime date)
        {
            return await _context.AppointmentSlots
                .Where(slot => slot.ConsultantID == consultantId 
                            && slot.SlotDateTime.Date == date.Date
                            && slot.Status == "Available"
                            && slot.SlotDateTime > DateTime.Now)
                .OrderBy(slot => slot.SlotDateTime)
                .ToListAsync();
        }

        public async Task<List<AppointmentSlot>> GetSlotsByDateRangeAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            return await _context.AppointmentSlots
                .Where(slot => slot.ConsultantID == consultantId 
                            && slot.SlotDateTime >= startDate 
                            && slot.SlotDateTime <= endDate)
                .OrderBy(slot => slot.SlotDateTime)
                .ToListAsync();
        }

        public async Task<AppointmentSlot?> GetSlotByDateTimeAsync(int consultantId, DateTime slotDateTime)
        {
            return await _context.AppointmentSlots
                .FirstOrDefaultAsync(slot => slot.ConsultantID == consultantId 
                                          && slot.SlotDateTime == slotDateTime);
        }

        public async Task<bool> IsSlotAvailableAsync(int consultantId, DateTime slotDateTime)
        {
            var slot = await GetSlotByDateTimeAsync(consultantId, slotDateTime);
            return slot?.Status == "Available";
        }

        public async Task GenerateSlotsForDateAsync(int consultantId, DateTime date)
        {
            // Get consultant's schedule for the day
            var dayOfWeek = (int)date.DayOfWeek;
            var schedule = await _context.ConsultantSchedules
                .FirstOrDefaultAsync(cs => cs.ConsultantID == consultantId 
                                        && cs.DayOfWeek == dayOfWeek 
                                        && cs.IsActive);

            if (schedule == null) return;

            // Check for availability exceptions
            var availability = await _context.ConsultantAvailabilities
                .FirstOrDefaultAsync(ca => ca.ConsultantID == consultantId 
                                        && ca.Date.Date == date.Date);

            TimeSpan startTime, endTime;
            if (availability != null && availability.Status == "Custom")
            {
                if (availability.StartTime == null || availability.EndTime == null) return;
                startTime = availability.StartTime.Value;
                endTime = availability.EndTime.Value;
            }
            else if (availability != null && availability.Status == "Unavailable")
            {
                return; // Don't generate slots for unavailable days
            }
            else
            {
                startTime = schedule.StartTime;
                endTime = schedule.EndTime;
            }

            // Generate time slots
            var currentTime = startTime;
            var slotDuration = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);

            while (currentTime.Add(slotDuration) <= endTime)
            {
                var slotDateTime = date.Date.Add(currentTime);

                // Check if slot already exists
                var existingSlot = await GetSlotByDateTimeAsync(consultantId, slotDateTime);
                if (existingSlot == null && slotDateTime > DateTime.Now)
                {
                    var newSlot = new AppointmentSlot
                    {
                        ConsultantID = consultantId,
                        SlotDateTime = slotDateTime,
                        DurationMinutes = schedule.SlotDurationMinutes,
                        Status = "Available",
                        CreatedAt = DateTime.Now
                    };

                    _context.AppointmentSlots.Add(newSlot);
                }

                currentTime = currentTime.Add(slotDuration);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<AppointmentSlot>> GetBookedSlotsByConsultantAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            return await _context.AppointmentSlots
                .Include(slot => slot.Appointment)
                .Where(slot => slot.ConsultantID == consultantId 
                            && slot.SlotDateTime >= startDate 
                            && slot.SlotDateTime <= endDate
                            && slot.Status == "Booked")
                .OrderBy(slot => slot.SlotDateTime)
                .ToListAsync();
        }
    }
}