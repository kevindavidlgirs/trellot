using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace prid_2021_g06.Models
{
    public static class DTOMappers
    {
        public static UserDTO ToDTO(this User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Pseudo = user.Pseudo,
                LastName = user.LastName,
                FirstName = user.FirstName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                PicturePath = user.PicturePath,
                Role = user.Role,
                Phones = user.Phones.ToDTO()
            };
        }

        public static List<UserDTO> ToDTO(this IEnumerable<User> users)
        {
            return users.Select(m => m.ToDTO()).ToList();
        }

        public static PhoneDTO ToDTO(this Phone phone)
        {
            return new PhoneDTO
            {
                PhoneId = phone.PhoneId,
                Type = phone.Type,
                Number = phone.Number
            };
        }

        public static List<PhoneDTO> ToDTO(this IEnumerable<Phone> phones)
        {
            return phones.Select(p => p.ToDTO()).ToList();
        }

        public static BoardDTO ToDTO(this Board board)
        {
            return new BoardDTO
            {
                Id = board.Id,
                Name = board.Name,
                Owner = board.Owner.ToDTO()
            };
        }

        public static List<BoardDTO> ToDTO(this IEnumerable<Board> boards)
        {
            return boards.Select(b => b.ToDTO()).ToList();
        }

        public static BoardListDTO ToDTO(this BoardList bl)
        {
            return new BoardListDTO
            {
                Id = bl.Id,
                Name = bl.Name,
                BoardId = bl.Board.Id,
                Cards = bl.Cards.ToDTO()
            };
        }

        public static List<BoardListDTO> ToDTO(this IEnumerable<BoardList> boards)
        {
            return boards.Select(b => b.ToDTO()).ToList();
        }

        public static CardDTO ToDTO(this Card c)
        {
            return new CardDTO
            {
                Id = c.Id,
                Name = c.Name,
                Owner = c.Owner.ToDTO(),
                boardListId = c.BoardList.Id,
                Users = c.Users.ToDTO(),
                Tags = c.Tags.ToDTO()
            };
        }

        public static List<CardDTO> ToDTO(this IEnumerable<Card> cards)
        {
            return cards.Select(c => c.ToDTO()).ToList();
        }

        public static UserBoardDTO ToDTO(this UserBoard ub)
        {
            return new UserBoardDTO
            {
                BoardId = ub.BoardId,
                User = ub.User.ToDTO(),
                UserIsInvitedOnTheBoard = ub.UserIsInvitedOnTheBoard,
                IsOwner = ub.IsOwner
            };
        }

        public static List<UserBoardDTO> ToDTO(this IEnumerable<UserBoard> ubl)
        {
            return ubl.Select(ub => ub.ToDTO()).ToList();
        }

        public static UserCardDTO ToDTO(this UserCard uc)
        {
            return new UserCardDTO
            {
                Card = uc.Card.ToDTO(),
                User = uc.User.ToDTO(),
            };
        }

        public static List<UserCardDTO> ToDTO(this IEnumerable<UserCard> ucl)
        {
            return ucl.Select(uc => uc.ToDTO()).ToList();
        }

        public static PostDTO ToDTO(this Post p)
        {
            return new PostDTO
            {
                Id = p.Id,
                Text = p.Text,
                PicturePath = p.PicturePath,
                Card = p.Card.ToDTO(),
                //Owner = p.Owner.ToDTO()
            };
        }

        public static List<PostDTO> ToDTO(this IEnumerable<Post> pl)
        {
            return pl.Select(p => p.ToDTO()).ToList();
        }

        public static List<TagDTO> ToDTO(this IEnumerable<Tag> tags)
        {
            return tags.Select(m => m.ToDTO()).ToList();
        }

        public static TagDTO ToDTO(this Tag t)
        {
            return new TagDTO
            {
                Id = t.Id,
                Name = t.Name,
               
            };
        }
    }
}
