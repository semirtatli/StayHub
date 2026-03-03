using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StayHub.Services.Hotel.Domain.Entities;
using StayHub.Services.Hotel.Domain.Enums;
using StayHub.Services.Hotel.Domain.ValueObjects;
using StayHub.Services.Hotel.Infrastructure.Persistence;

namespace StayHub.Services.Hotel.Infrastructure.Seed;

/// <summary>
/// Seeds the Hotel database with realistic demo data.
/// Idempotent — skips if hotels already exist.
/// </summary>
public static class HotelSeeder
{
    private const string SystemAdminId = "system-seed-admin";
    private const string DemoOwnerId = "demo-hotel-owner";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HotelDbContext>>();

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

            if (await dbContext.Hotels.AnyAsync())
            {
                logger.LogInformation("Hotels already seeded — skipping");
                return;
            }

            logger.LogInformation("Seeding hotel demo data...");

            var hotels = CreateHotels();
            foreach (var hotel in hotels)
            {
                dbContext.Hotels.Add(hotel);
            }

            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} hotels successfully", hotels.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Hotel seeding failed — continuing without seed data");
        }
    }

    private static List<HotelEntity> CreateHotels()
    {
        return
        [
            CreateGrandIstanbul(),
            CreateBosphorusPalace(),
            CreateCappadociaCaveResort(),
            CreateAntalyaBeachResort(),
            CreateParisianBoutique(),
            CreateLondonRoyalHotel(),
            CreateTokyoGardenHotel(),
            CreateNewYorkSkylineHotel(),
            CreateDubaiLuxuryResort(),
            CreateBarcelonaCoastalInn(),
            CreateRomeCentroHotel(),
            CreateSantoriniSunsetVilla(),
        ];
    }

    // ── Hotel 1: Grand Istanbul ────────────────────────────
    private static HotelEntity CreateGrandIstanbul()
    {
        var hotel = HotelEntity.Create(
            "Grand Istanbul Hotel & Spa",
            "Experience the magic of Istanbul at our 5-star luxury hotel. Located in the heart of Sultanahmet, steps away from the Blue Mosque and Hagia Sophia. Featuring a world-class spa, rooftop restaurant with panoramic Bosphorus views, and impeccable Turkish hospitality.",
            5,
            Address.Create("Sultanahmet Meydanı No:12", "Istanbul", "Istanbul", "Turkey", "34122"),
            ContactInfo.Create("+90-212-555-0001", "info@grandistanbul.com", "https://grandistanbul.com"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(41.0054, 28.9768));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800");

        var deluxeKing = hotel.AddRoom("Deluxe King Room", "Elegant room with king bed, marble bathroom, and city views.", RoomType.Deluxe, 2, Money.Create(280.00m, "USD"), 15);
        deluxeKing.SetSize(42m);
        deluxeKing.SetBedConfiguration("1 King Bed");
        deluxeKing.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Room Service", "Flat Screen TV", "Safe", "Bathrobe"]);

        var suite = hotel.AddRoom("Bosphorus Suite", "Luxurious suite with separate living area and stunning Bosphorus views.", RoomType.Suite, 3, Money.Create(520.00m, "USD"), 5);
        suite.SetSize(75m);
        suite.SetBedConfiguration("1 King Bed + 1 Sofa Bed");
        suite.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Room Service", "Flat Screen TV", "Safe", "Bathrobe", "Balcony", "Jacuzzi"]);

        var standard = hotel.AddRoom("Standard Twin", "Comfortable room with twin beds, perfect for friends or colleagues.", RoomType.Twin, 2, Money.Create(180.00m, "USD"), 20);
        standard.SetSize(30m);
        standard.SetBedConfiguration("2 Twin Beds");
        standard.SetAmenities(["WiFi", "Air Conditioning", "Flat Screen TV", "Safe"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 2: Bosphorus Palace ──────────────────────────
    private static HotelEntity CreateBosphorusPalace()
    {
        var hotel = HotelEntity.Create(
            "Bosphorus Palace Hotel",
            "A restored Ottoman-era waterfront mansion offering boutique luxury on the shores of the Bosphorus. Each room tells a story of Istanbul's rich heritage, with antique furnishings and modern comforts.",
            4,
            Address.Create("Yahya Kemal Caddesi No:28", "Istanbul", "Istanbul", "Turkey", "34470"),
            ContactInfo.Create("+90-212-555-0002", "reservations@bosphoruspalace.com", "https://bosphoruspalace.com"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(41.0422, 29.0345));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800");

        var deluxe = hotel.AddRoom("Heritage Deluxe", "Beautifully restored room with Ottoman-era décor and waterfront views.", RoomType.Deluxe, 2, Money.Create(350.00m, "USD"), 8);
        deluxe.SetSize(48m);
        deluxe.SetBedConfiguration("1 King Bed");
        deluxe.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Sea View", "Antique Furnishings"]);

        var penthouse = hotel.AddRoom("Sultan Penthouse", "The crown jewel — a lavish penthouse with 360° Bosphorus views.", RoomType.Penthouse, 4, Money.Create(1200.00m, "USD"), 1);
        penthouse.SetSize(150m);
        penthouse.SetBedConfiguration("2 King Beds");
        penthouse.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Room Service", "Private Terrace", "Jacuzzi", "Butler Service"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 3: Cappadocia Cave Resort ────────────────────
    private static HotelEntity CreateCappadociaCaveResort()
    {
        var hotel = HotelEntity.Create(
            "Cappadocia Cave Resort",
            "Stay in authentic cave rooms carved into Cappadocia's fairy chimneys. Wake up to hot air balloons at sunrise, enjoy Turkish breakfast on our panoramic terrace, and explore the magical landscape.",
            4,
            Address.Create("Göreme Kasabası No:5", "Nevşehir", "Cappadocia", "Turkey", "50180"),
            ContactInfo.Create("+90-384-555-0003", "hello@cappadociacave.com", "https://cappadociacave.com"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(38.6431, 34.8283));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1570213489059-0aac6626cade?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1570213489059-0aac6626cade?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800");

        var caveRoom = hotel.AddRoom("Cave Deluxe", "Authentic cave room with modern comforts and fairy chimney views.", RoomType.Deluxe, 2, Money.Create(220.00m, "USD"), 12);
        caveRoom.SetSize(35m);
        caveRoom.SetBedConfiguration("1 Queen Bed");
        caveRoom.SetAmenities(["WiFi", "Heating", "Mini Bar", "Terrace", "Valley View"]);

        var caveSuite = hotel.AddRoom("King Cave Suite", "Expansive cave suite with private terrace and outdoor seating.", RoomType.Suite, 2, Money.Create(400.00m, "USD"), 4);
        caveSuite.SetSize(65m);
        caveSuite.SetBedConfiguration("1 King Bed");
        caveSuite.SetAmenities(["WiFi", "Heating", "Mini Bar", "Private Terrace", "Fireplace", "Jacuzzi"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 4: Antalya Beach Resort ──────────────────────
    private static HotelEntity CreateAntalyaBeachResort()
    {
        var hotel = HotelEntity.Create(
            "Antalya Riviera Beach Resort",
            "All-inclusive beachfront resort on the stunning Turkish Riviera. Features private beach, multiple pools, water sports, kids' club, and 5 restaurants serving world cuisines.",
            5,
            Address.Create("Lara Turizm Yolu No:88", "Antalya", "Antalya", "Turkey", "07230"),
            ContactInfo.Create("+90-242-555-0004", "booking@antalyariviera.com", "https://antalyariviera.com"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(36.8529, 30.7956));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800");

        var standard = hotel.AddRoom("Sea View Standard", "Bright room with Mediterranean sea views and balcony.", RoomType.Double, 2, Money.Create(160.00m, "USD"), 40);
        standard.SetSize(32m);
        standard.SetBedConfiguration("1 Double Bed");
        standard.SetAmenities(["WiFi", "Air Conditioning", "Balcony", "Sea View", "All-Inclusive"]);

        var family = hotel.AddRoom("Family Suite", "Spacious suite with separate children's area and garden view.", RoomType.Family, 5, Money.Create(320.00m, "USD"), 15);
        family.SetSize(65m);
        family.SetBedConfiguration("1 King Bed + 2 Single Beds");
        family.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Kids Area", "Garden View", "All-Inclusive"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 5: Parisian Boutique ─────────────────────────
    private static HotelEntity CreateParisianBoutique()
    {
        var hotel = HotelEntity.Create(
            "Le Marais Boutique Hotel",
            "Charming boutique hotel in the heart of Le Marais, Paris. Housed in a beautifully renovated 18th-century townhouse, just minutes from the Louvre, Notre-Dame, and the best cafés.",
            4,
            Address.Create("15 Rue des Archives", "Paris", "Île-de-France", "France", "75004"),
            ContactInfo.Create("+33-1-42-555-005", "contact@lemaraisboutique.fr", "https://lemaraisboutique.fr"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(48.8566, 2.3522));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1549294413-26f195200c16?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1549294413-26f195200c16?w=800");

        var classic = hotel.AddRoom("Classic Parisienne", "Elegant room with Haussmann-style decor and courtyard views.", RoomType.Double, 2, Money.Create(240.00m, "EUR"), 10);
        classic.SetSize(25m);
        classic.SetBedConfiguration("1 Queen Bed");
        classic.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Nespresso Machine"]);

        var prestige = hotel.AddRoom("Prestige Suite", "Top-floor suite with Eiffel Tower glimpses and private lounge.", RoomType.Suite, 2, Money.Create(580.00m, "EUR"), 3);
        prestige.SetSize(55m);
        prestige.SetBedConfiguration("1 King Bed");
        prestige.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Room Service", "Eiffel Tower View", "Lounge Access"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 6: London Royal Hotel ────────────────────────
    private static HotelEntity CreateLondonRoyalHotel()
    {
        var hotel = HotelEntity.Create(
            "The Royal Kensington",
            "A distinguished 5-star hotel in South Kensington, London. Walking distance to the V&A Museum, Natural History Museum, and Hyde Park. Traditional British elegance meets contemporary luxury.",
            5,
            Address.Create("22 Queen's Gate", "London", "England", "United Kingdom", "SW7 5EX"),
            ContactInfo.Create("+44-20-7555-0006", "reservations@royalkensington.co.uk", "https://royalkensington.co.uk"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(51.4952, -0.1789));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800");

        var superior = hotel.AddRoom("Superior King", "Refined room with en-suite bathroom and garden views.", RoomType.Deluxe, 2, Money.Create(320.00m, "GBP"), 20);
        superior.SetSize(35m);
        superior.SetBedConfiguration("1 King Bed");
        superior.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Afternoon Tea Service", "Flat Screen TV"]);

        var royal = hotel.AddRoom("Royal Suite", "Our finest suite with separate living room and butler service.", RoomType.Suite, 3, Money.Create(850.00m, "GBP"), 2);
        royal.SetSize(90m);
        royal.SetBedConfiguration("1 Emperor Bed + 1 Sofa Bed");
        royal.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Butler Service", "Champagne on Arrival", "Park View"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 7: Tokyo Garden Hotel ────────────────────────
    private static HotelEntity CreateTokyoGardenHotel()
    {
        var hotel = HotelEntity.Create(
            "Tokyo Zen Garden Hotel",
            "A serene oasis in bustling Shinjuku. Japanese-inspired minimalist design with a tranquil zen garden, onsen bath, and rooftop bar with views of Mount Fuji on clear days.",
            4,
            Address.Create("3-14-1 Nishi-Shinjuku", "Tokyo", "Tokyo", "Japan", "160-0023"),
            ContactInfo.Create("+81-3-5555-0007", "stay@tokyozengarden.jp", "https://tokyozengarden.jp"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(35.6895, 139.6917));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800");

        var zen = hotel.AddRoom("Zen Single", "Minimalist Japanese room with futon bedding and tatami floors.", RoomType.Single, 1, Money.Create(18000m, "JPY"), 25);
        zen.SetSize(20m);
        zen.SetBedConfiguration("1 Japanese Futon");
        zen.SetAmenities(["WiFi", "Air Conditioning", "Green Tea Set", "Yukata Robe", "Onsen Access"]);

        var sakura = hotel.AddRoom("Sakura Suite", "Premium suite with private garden view and soaking tub.", RoomType.Suite, 2, Money.Create(45000m, "JPY"), 4);
        sakura.SetSize(55m);
        sakura.SetBedConfiguration("1 King Bed");
        sakura.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Private Garden View", "Soaking Tub", "Onsen Access"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 8: New York Skyline Hotel ────────────────────
    private static HotelEntity CreateNewYorkSkylineHotel()
    {
        var hotel = HotelEntity.Create(
            "Manhattan Skyline Hotel",
            "Modern luxury in Midtown Manhattan with floor-to-ceiling windows and breathtaking skyline views. Steps from Times Square, Broadway, and Central Park. Rooftop bar and fitness center included.",
            4,
            Address.Create("250 West 43rd Street", "New York", "NY", "United States", "10036"),
            ContactInfo.Create("+1-212-555-0008", "info@manhattanskyline.com", "https://manhattanskyline.com"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(40.7580, -73.9855));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1455587734955-081b22074882?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1455587734955-081b22074882?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1618773928121-c32242e63f39?w=800");

        var city = hotel.AddRoom("City View King", "Stylish room with panoramic Manhattan skyline views.", RoomType.Deluxe, 2, Money.Create(350.00m, "USD"), 30);
        city.SetSize(35m);
        city.SetBedConfiguration("1 King Bed");
        city.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Skyline View", "Flat Screen TV", "Fitness Center Access"]);

        var studio = hotel.AddRoom("Broadway Studio", "Open-plan studio with kitchenette, perfect for extended stays.", RoomType.Studio, 2, Money.Create(420.00m, "USD"), 10);
        studio.SetSize(45m);
        studio.SetBedConfiguration("1 Queen Bed");
        studio.SetAmenities(["WiFi", "Air Conditioning", "Kitchenette", "Washer/Dryer", "Work Desk"]);

        var pent = hotel.AddRoom("Empire Penthouse", "Spectacular penthouse with private terrace and Empire State Building views.", RoomType.Penthouse, 4, Money.Create(1500.00m, "USD"), 1);
        pent.SetSize(120m);
        pent.SetBedConfiguration("2 King Beds");
        pent.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Private Terrace", "Jacuzzi", "Butler Service", "Empire State View"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 9: Dubai Luxury Resort ───────────────────────
    private static HotelEntity CreateDubaiLuxuryResort()
    {
        var hotel = HotelEntity.Create(
            "Palm Jumeirah Royal Resort",
            "Ultra-luxury beachfront resort on Dubai's iconic Palm Jumeirah. Featuring private beach, infinity pools, underwater restaurant, and direct views of the Arabian Gulf.",
            5,
            Address.Create("Crescent Road, Palm Jumeirah", "Dubai", "Dubai", "United Arab Emirates", "00000"),
            ContactInfo.Create("+971-4-555-0009", "concierge@palmroyalresort.ae", "https://palmroyalresort.ae"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(25.1124, 55.1390));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800");

        var ocean = hotel.AddRoom("Ocean Deluxe", "Luxurious room with floor-to-ceiling ocean views and private balcony.", RoomType.Deluxe, 2, Money.Create(450.00m, "USD"), 25);
        ocean.SetSize(50m);
        ocean.SetBedConfiguration("1 King Bed");
        ocean.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Ocean View", "Balcony", "Premium Toiletries"]);

        var villa = hotel.AddRoom("Beach Villa", "Private beachfront villa with plunge pool and dedicated butler.", RoomType.Suite, 4, Money.Create(2000.00m, "USD"), 3);
        villa.SetSize(200m);
        villa.SetBedConfiguration("2 King Beds");
        villa.SetAmenities(["WiFi", "Air Conditioning", "Full Kitchen", "Private Pool", "Butler Service", "Beach Access", "Outdoor Shower"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 10: Barcelona Coastal Inn ────────────────────
    private static HotelEntity CreateBarcelonaCoastalInn()
    {
        var hotel = HotelEntity.Create(
            "Barcelona Gothic Quarter Inn",
            "Cozy boutique hotel nestled in Barcelona's atmospheric Gothic Quarter. Walk to La Rambla, Sagrada Família, and the beach. Rooftop tapas bar with cathedral views.",
            3,
            Address.Create("Carrer dels Banys Nous 14", "Barcelona", "Catalonia", "Spain", "08002"),
            ContactInfo.Create("+34-93-555-0010", "hola@gothicquarterinn.es", "https://gothicquarterinn.es"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(41.3818, 2.1768));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800");

        var cozy = hotel.AddRoom("Cozy Double", "Charming room with exposed brick and modern bathroom.", RoomType.Double, 2, Money.Create(120.00m, "EUR"), 12);
        cozy.SetSize(22m);
        cozy.SetBedConfiguration("1 Double Bed");
        cozy.SetAmenities(["WiFi", "Air Conditioning", "Flat Screen TV"]);

        var terrace = hotel.AddRoom("Terrace Room", "Corner room with private terrace overlooking the Gothic Quarter.", RoomType.Deluxe, 2, Money.Create(195.00m, "EUR"), 4);
        terrace.SetSize(30m);
        terrace.SetBedConfiguration("1 Queen Bed");
        terrace.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Private Terrace", "Cathedral View"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 11: Rome Centro Hotel ────────────────────────
    private static HotelEntity CreateRomeCentroHotel()
    {
        var hotel = HotelEntity.Create(
            "Roma Centro Palazzo",
            "Elegant hotel housed in a Renaissance palazzo near the Trevi Fountain and Pantheon. Classic Italian design with a rooftop garden restaurant and wine cellar.",
            4,
            Address.Create("Via del Corso 126", "Rome", "Lazio", "Italy", "00186"),
            ContactInfo.Create("+39-06-555-0011", "prenotazioni@romacentro.it", "https://romacentro.it"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(41.8986, 12.4769));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800");

        var classica = hotel.AddRoom("Camera Classica", "Traditional Italian room with frescoed ceiling and marble floor.", RoomType.Double, 2, Money.Create(200.00m, "EUR"), 18);
        classica.SetSize(28m);
        classica.SetBedConfiguration("1 Queen Bed");
        classica.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Espresso Machine"]);

        var terrazza = hotel.AddRoom("Terrazza Suite", "Rooftop suite with private terrace and views of Roman rooftops.", RoomType.Suite, 2, Money.Create(450.00m, "EUR"), 2);
        terrazza.SetSize(60m);
        terrazza.SetBedConfiguration("1 King Bed");
        terrazza.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Room Service", "Private Terrace", "Panoramic View"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }

    // ── Hotel 12: Santorini Sunset Villa ───────────────────
    private static HotelEntity CreateSantoriniSunsetVilla()
    {
        var hotel = HotelEntity.Create(
            "Santorini Caldera Villas",
            "Whitewashed clifftop villas perched on Santorini's famous caldera. Each villa features a private infinity pool, sunset views, and Cycladic architecture. The most romantic escape in Greece.",
            5,
            Address.Create("Oia Caldera Path", "Santorini", "South Aegean", "Greece", "84702"),
            ContactInfo.Create("+30-22860-55012", "info@calderavillas.gr", "https://calderavillas.gr"),
            DemoOwnerId);

        hotel.SetLocation(GeoLocation.Create(36.4618, 25.3753));
        hotel.SetCoverImage("https://images.unsplash.com/photo-1602343168117-bb8ffe3e2e9f?w=800");
        hotel.AddPhotoUrl("https://images.unsplash.com/photo-1602343168117-bb8ffe3e2e9f?w=800");

        var calderaRoom = hotel.AddRoom("Caldera View Room", "Iconic whitewashed room with blue-domed views and private balcony.", RoomType.Deluxe, 2, Money.Create(380.00m, "EUR"), 8);
        calderaRoom.SetSize(35m);
        calderaRoom.SetBedConfiguration("1 King Bed");
        calderaRoom.SetAmenities(["WiFi", "Air Conditioning", "Mini Bar", "Caldera View", "Balcony"]);

        var sunsetVilla = hotel.AddRoom("Sunset Infinity Villa", "Private villa with infinity pool, outdoor dining, and the most spectacular sunset view in Oia.", RoomType.Suite, 4, Money.Create(950.00m, "EUR"), 2);
        sunsetVilla.SetSize(100m);
        sunsetVilla.SetBedConfiguration("1 King Bed + 1 Queen Bed");
        sunsetVilla.SetAmenities(["WiFi", "Air Conditioning", "Full Kitchen", "Private Infinity Pool", "Outdoor Dining", "Sunset View", "Concierge"]);

        hotel.SubmitForApproval();
        hotel.Approve(SystemAdminId);
        return hotel;
    }
}
