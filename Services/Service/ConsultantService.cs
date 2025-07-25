using BussinessObjects;
using Repositories.IRepository;
using Repositories.IRepository.Consultants;
using Repositories.IRepository.Users;
using Services.DTOs.Consultant;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Service
{
    public class ConsultantService : IConsultantService
    {
        private readonly IConsultantRepository _consultantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ConsultantService(IConsultantRepository consultantRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _consultantRepository = consultantRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ConsultantResponse> CreateConsultant(ConsultantRequest consultantRequest)
        {
            if (consultantRequest == null)
            {
                return null;
            }
            var user = await _userRepository.GetByIdAsync(consultantRequest.ConsultantID);
            if (user == null)
            {
                return null;
            }

            var consultant = new Consultant();
            consultant.Specialty = consultantRequest.Specialty;
            consultant.Qualifications = consultantRequest.Qualifications;
            consultant.WorkingHours = consultantRequest.WorkingHours;
            consultant.User = user;
            await _consultantRepository.InsertAsync(consultant);
            await _unitOfWork.SaveAsync();

            var consultantResponse = new ConsultantResponse
            {
                ConsultantID = consultantRequest.ConsultantID,
                Qualifications = consultantRequest.Qualifications,
                Specialty = consultantRequest.Specialty,
                WorkingHours = consultantRequest.WorkingHours
            };
            return consultantResponse;
        }

        public async Task<List<DateTime>> GetWorkingHour(int id)
        {
            var consultant =  _consultantRepository.GetById(id);
            if (consultant == null)
            {
                return null;
            }
          
            return consultant.WorkingHours;
        }
    }
}
