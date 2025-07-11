using AutoMapper;
using chatgptbot.dto;
using chatgptbot.Entities;

namespace chatgptbot.Profiles
{
    public class GptProfile : Profile
    {
        public GptProfile()
        {
            //Source -> Target (Database -> Client)
            CreateMap<Assistants, AssistantDto>().ReverseMap();
            //Create Target -> Source  (Client -> Databasase)

        }
    }
}