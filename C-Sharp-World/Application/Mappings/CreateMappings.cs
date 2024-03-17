using AutoMapper;
using IdentityVerificationService.Domain.DTOs.IdVerification;
using IdentityVerificationService.Domain.DTOs.IdVerification.Response;
using IdentityVerificationService.Domain.Entities.IdVerification;
using Newtonsoft.Json;

namespace IdentityVerificationService.Application.Mappings
{
    public class CreateMappings
    {
        protected CreateMappings()
        {
        }

        public static MapperConfiguration CreateConfiguration()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TruliooTransactionStatusDto, IDVerificationTransactionStatus>()
                .ForMember(x => x.Id, opt => opt.MapFrom((source, destination) => destination.Id))
                .ForMember(x => x.TransactionId, opt => opt.MapFrom((source) => source.TransactionId))
                .ForMember(x => x.Status, opt => opt.MapFrom((source) => JsonConvert.SerializeObject(source)))
                .ForMember(x => x.CreatedDate, opt => opt.MapFrom((source) => DateTime.UtcNow))
                .ForMember(x => x.CreatedBy, opt => opt.MapFrom((source) => "Wrapper-API"));

                cfg.CreateMap<ImagesUrlDto, IDVerificationImage>()
                .ForMember(x => x.DocumentFrontImage, opt => opt.MapFrom((source, destination) => source.DocumentFrontImage != null ? source.DocumentFrontImage : destination.DocumentFrontImage))
                .ForMember(x => x.DocumentBackImage, opt => opt.MapFrom((source, destination) => source.DocumentBackImage != null ? source.DocumentBackImage : destination.DocumentBackImage))
                .ForMember(x => x.SelfieImage, opt => opt.MapFrom((source, destination) => source.SelfieImage != null ? source.SelfieImage : destination.SelfieImage))
                .ReverseMap();

                cfg.CreateMap<TruliooImageDto, IDVerificationImage>().ReverseMap();

                cfg.CreateMap<TruliooDocumentVerificationDto, IDVerificationDocument>().ReverseMap();
            });

            return mapperConfiguration;
        }
    }
}
