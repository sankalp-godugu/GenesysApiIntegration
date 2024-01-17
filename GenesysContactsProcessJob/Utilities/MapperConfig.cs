using AutoMapper;
using GenesysContactsProcessJob.Model.DTO;
using GenesysContactsProcessJob.Model.Request;
using Microsoft.Extensions.Configuration;

namespace GenesysContactsProcessJob.Utilities
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper(IConfiguration configuration)
        {
            //Provide all the Mapping Configuration
            MapperConfiguration config = new(cfg =>
            {
                //Configuring PostDischargeInfo to AddContactsRequest
                _ = cfg.CreateMap<PostDischargeInfo_GenesysContactInfo, PostContactsRequest>()
                    .ForMember(acr => acr.Id, opt => opt.MapFrom(src => src.PostDischargeId))
                    .ForMember(acr => acr.ContactListId, opt => opt.MapFrom(src => src.Language == Languages.English ?
                configuration["Genesys:AppConfigurations:ContactListId:AetnaEnglish"] : configuration["Genesys:AppConfigurations:ContactListId:AetnaSpanish"]))
                    .ForPath(acr => acr.Data.NhMemberId, opt => opt.MapFrom(src => src.NHMemberId))
                    .ForPath(acr => acr.Data.MemberName, opt => opt.MapFrom(src => src.MemberName))
                    .ForPath(acr => acr.Data.Language, opt => opt.MapFrom(src => src.Language))
                    .ForPath(acr => acr.Data.Address, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Address1) ? src.Address2 : src.Address1))
                    .ForPath(acr => acr.Data.Region, opt => opt.MapFrom(src => src.Region))
                    .ForPath(acr => acr.Data.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNbr))
                    .ForPath(acr => acr.Data.CarrierName, opt => opt.MapFrom(src => src.CarrierName))
                    .ForPath(acr => acr.Data.LoadDate, opt => opt.MapFrom(src => src.LoadDate))
                    .ForPath(acr => acr.Data.DischargeDate, opt => opt.MapFrom(src => src.DischargeDate))
                    .ForPath(acr => acr.Data.DayCount, opt => opt.MapFrom(src => src.DayCount))
                    .ForPath(acr => acr.Data.AttemptCountToday, opt => opt.MapFrom(src => src.AttemptCountToday))
                    .ForPath(acr => acr.Data.AttemptCountTotal, opt =>
                    {
                        //opt.Condition(c => c.Source.DayCount > 1);
                        opt.MapFrom(src => src.AttemptCountTotal);
                    });

                // UpdateContactsRequest
                _ = cfg.CreateMap<PostDischargeInfo_GenesysContactInfo, PostContactsRequest>()
                    .ForMember(ucr => ucr.Id, opt => opt.MapFrom(src => src.PostDischargeId))
                    .ForMember(ucr => ucr.ContactListId, opt => opt.MapFrom(src => src.Language == Languages.English ?
                configuration["Genesys:AppConfigurations:ContactListId:AetnaEnglish"] : configuration["Genesys:AppConfigurations:ContactListId:AetnaSpanish"]))
                    .ForPath(ucr => ucr.Data.NhMemberId, opt => opt.MapFrom(src => src.NHMemberId))
                    .ForPath(ucr => ucr.Data.MemberName, opt => opt.MapFrom(src => src.MemberName))
                    .ForPath(ucr => ucr.Data.Language, opt => opt.MapFrom(src => src.Language))
                    .ForPath(ucr => ucr.Data.Address, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Address1) ? src.Address2 : src.Address1))
                    .ForPath(ucr => ucr.Data.Region, opt => opt.MapFrom(src => src.Region))
                    .ForPath(ucr => ucr.Data.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNbr))
                    .ForPath(ucr => ucr.Data.CarrierName, opt => opt.MapFrom(src => src.CarrierName))
                    .ForPath(ucr => ucr.Data.LoadDate, opt => opt.MapFrom(src => src.LoadDate))
                    .ForPath(ucr => ucr.Data.DischargeDate, opt => opt.MapFrom(src => src.DischargeDate))
                    .ForPath(ucr => ucr.Data.DayCount, opt => opt.MapFrom(src => src.DayCount))
                    .ForPath(ucr => ucr.Data.AttemptCountToday, opt => opt.MapFrom(src => src.AttemptCountToday))
                    .ForPath(ucr => ucr.Data.AttemptCountTotal, opt =>
                    {
                        //opt.Condition(c => c.Source.DayCount <= 1);
                        opt.MapFrom(src => src.AttemptCountTotal);
                    });
            });

            //Create an Instance of Mapper and return that Instance
            Mapper mapper = new(config);
            return mapper;
        }
    }
}
