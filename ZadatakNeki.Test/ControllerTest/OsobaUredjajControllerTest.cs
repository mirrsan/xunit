using Autofac.Extras.Moq;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ZadatakNeki.Controllers;
using ZadatakNeki.DTO;
using ZadatakNeki.Models;
using ZadatakNeki.Repositorys.IRepository;

namespace ZadatakNeki.Test.ControllerTest
{
    public class OsobaUredjajControllerTest
    {
        public List<OsobaUredjaj> ListOsobaUredjaj()
        {
            List<OsobaUredjaj> lista = new List<OsobaUredjaj>()
            {
                new OsobaUredjaj()

                {
                    PocetakKoriscenja = DateTime.MaxValue,
                    Osoba = new Osoba() {Ime = "mirsan"},
                    Uredjaj = new Uredjaj() {Naziv = "fleska"}
                },
                new OsobaUredjaj()
                {
                    Osoba = new Osoba() {Ime = "ena"},
                    Uredjaj = new Uredjaj() {Naziv = "laptop"}
                }
            };
            return lista;
        }

        public List<OsobaUredjajDTO> ListOsobaUredjajDto()
        {
            List<OsobaUredjajDTO> lista = new List<OsobaUredjajDTO>()
            {
                new OsobaUredjajDTO()
                {
                    Osoba = new OsobaDTO() {Ime = "mirsan"},
                    Uredjaj = new UredjajDTO() {Naziv = "fleska"}
                },
                new OsobaUredjajDTO()
                {
                    Osoba = new OsobaDTO() {Ime = "mirsan"},
                    Uredjaj = new UredjajDTO() {Naziv = "fleska"}
                }
            };
            return lista;
        }

        [Fact]
        public void Sve_KadRadi()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(ListOsobaUredjaj);
                mock.Mock<IMapper>().Setup(e => e.Map<OsobaUredjajDTO>(It.IsAny<OsobaUredjaj>()))
                    .Returns(ListOsobaUredjajDto()[0]);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.Sve();
                var ocekujes = ListOsobaUredjajDto();

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Theory]
        [InlineData(2)]
        public void PoId_KadVracaOsobaUredjaj(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(ListOsobaUredjaj()[0]);
                mock.Mock<IMapper>().Setup(e => e.Map<OsobaUredjajDTO>(It.IsAny<OsobaUredjaj>()))
                    .Returns(ListOsobaUredjajDto()[0]);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.PoId(id);
                var ocekujes = ListOsobaUredjajDto()[0];

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Theory]
        [InlineData(7)]
        public void PoId_KadOsobaUredjajNijePronadjen(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(() => null);

                var obj = mock.Create<OsobaUredjajController>();

                var ocekujes = "Nema ti toga vamo.";
                var dobijas = obj.PoId(id);

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void DodatiNovi_KadJeUlazniParametarNull()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.DodatiNovi(null);
                var ocekujes = "aha ne moze to tako e";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void DodatiNovi_AkoSeUredjajVecKoristi()
        {
            using (var mock = AutoMock.GetLoose())
            {
                List<OsobaUredjaj> list = new List<OsobaUredjaj>()
                {
                    new OsobaUredjaj()
                    {
                        Uredjaj = new Uredjaj() {Naziv = "upaljac"},
                        KrajKoriscenja = DateTime.MaxValue
                    }
                };
                var ouDto = new OsobaUredjajDTO()
                {
                    Osoba = new OsobaDTO() { Ime = "mirsan"},
                    Uredjaj = new UredjajDTO() {Naziv = "upaljac"}
                };

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(list);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.DodatiNovi(ouDto);
                var ocekujes = $"{ouDto.Osoba.Ime} ne moze koristiti ovaj uredjaj zato sto ga vec neko koristi.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void DodatiNovi_KadSacuvaEntitet()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IMapper>().Setup(e => e.Map<OsobaUredjaj>(It.IsAny<OsobaUredjajDTO>()))
                    .Returns(ListOsobaUredjaj()[0]);

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(ListOsobaUredjaj);

                mock.Mock<IOsobaRepository>().Setup(e => e.DajSveEntitete())
                    .Returns(new List<Osoba>() {new Osoba() {Ime = "mirsan"}});

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.DodatiNovi(ListOsobaUredjajDto()[0]);
                var ocekujes = "Sacuvano je ;)";

                mock.Mock<IOsobaUredjajRepository>()
                    .Verify(e => e.DodajEntitet(It.IsAny<OsobaUredjaj>()), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(1)]
        public void MenjanjeEntiteta_KadNemaTrazenogEntitetaZaMenjaje(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(() => null);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.MenjanjeEntiteta(id, ListOsobaUredjajDto()[0]);
                var ocekujes = "Nema sa tim ID nista.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(1)]
        public void MenjanjeEntiteta_KadSeUredjajVecKoristi(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                List<OsobaUredjaj> list = new List<OsobaUredjaj>()
                {
                    new OsobaUredjaj()
                    {
                        Uredjaj = new Uredjaj() {Naziv = "upaljac"},
                        KrajKoriscenja = DateTime.MaxValue
                    }
                };
                var ouDto = new OsobaUredjajDTO()
                {
                    Osoba = new OsobaDTO() { Ime = "mirsan" },
                    Uredjaj = new UredjajDTO() { Naziv = "upaljac" }
                };

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(ListOsobaUredjaj()[0]);

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(list);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.MenjanjeEntiteta(id, ouDto);
                var ocekujes = $"{ouDto.Osoba.Ime} ne moze koristiti ovaj uredjaj zato sto ga vec neko koristi.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(1)]
        public void MenjanjeEntiteta_KadSacuvaIzmene(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IMapper>().Setup(e => e.Map<Osoba>(It.IsAny<OsobaDTO>()))
                    .Returns(new Osoba() {Ime = "mirsan"} );

                mock.Mock<IMapper>().Setup(e => e.Map<Uredjaj>(It.IsAny<UredjajDTO>()))
                    .Returns(new Uredjaj() { Naziv = "tel" });

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(ListOsobaUredjaj()[0]);

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(ListOsobaUredjaj);

                mock.Mock<IOsobaRepository>().Setup(e => e.DajSveEntitete())
                    .Returns(new List<Osoba>() { new Osoba() { Ime = "mirsan" } });

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.MenjanjeEntiteta(id, ListOsobaUredjajDto()[0]);
                var ocekujes = "Zamenjemo.";

                mock.Mock<IUnitOfWork>().Verify(e => e.Sacuvaj(), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(2)]
        public void BrisanjeEntiteta_KadNemaEntitetaSaDatimID(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(() => null);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.BrisanjeEntiteta(id);
                var ocekujes = "Sta ces brisat, njega nako nije ni bilo.";

                var result = Assert.IsType<NotFoundObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(2)]
        public void BrisanjeEntiteta_KadObriseEntitet(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(ListOsobaUredjaj()[0]);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.BrisanjeEntiteta(id);
                var ocekujes = "Obrisao sam.";

                mock.Mock<IOsobaUredjajRepository>()
                    .Verify(e => e.ObrisiEntitet(It.IsAny<OsobaUredjaj>()), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void PretragaPoPocetku_KadRadi()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete())
                    .Returns(ListOsobaUredjaj());

                mock.Mock<IMapper>().Setup(e => e.Map<OsobaUredjajDTO>(It.IsAny<OsobaUredjaj>()))
                    .Returns(ListOsobaUredjajDto()[0]);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.PretragaPoPocetku(DateTime.MinValue);
                var ocekujes = new List<OsobaUredjajDTO>() {ListOsobaUredjajDto()[0]};

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Fact]
        public void PretragaPocetakKraj_KadRadi()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var pocetak = DateTime.MinValue;
                var kraj = DateTime.MaxValue;

                var lista = new List<OsobaUredjaj>()
                {
                    new OsobaUredjaj()
                    {
                        Id = 1,
                        PocetakKoriscenja = pocetak,
                        KrajKoriscenja = kraj
                    }
                };

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete())
                    .Returns(lista);

                mock.Mock<IMapper>().Setup(e => e.Map<OsobaUredjajDTO>(It.IsAny<OsobaUredjaj>()))
                    .Returns(ListOsobaUredjajDto()[0]);

                var obj = mock.Create<OsobaUredjajController>();

                var dobijas = obj.PretragaPocetakKraj(pocetak, kraj);
                var ocekujes = new List<OsobaUredjajDTO>() { ListOsobaUredjajDto()[0] };

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }
    }
}
    

