using AutoMapper;
using BaldurBillsApp.Models;
using BaldurBillsApp.ViewModels;

namespace BaldurBillsApp.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Prepayment, PrepaymentViewModel>();
        }
    }
}
