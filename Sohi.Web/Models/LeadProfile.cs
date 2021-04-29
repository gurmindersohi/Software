using System;
using AutoMapper;
using Sohi.Models;

namespace Sohi.Web.Models
{
    public class LeadProfile : Profile
    {
        public LeadProfile()
        {
            CreateMap<Lead, EditLeadModel>()
                .ForMember(dest => dest.ConfirmEmail,
                           opt => opt.MapFrom(src => src.Email));

            CreateMap<EditLeadModel, Lead>();
        }
    }
}
