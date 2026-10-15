using System;
using System.Collections.Generic;
using System.Linq;

namespace KrishiBondhu.Domain.Common;

public record DistrictLocation(
    string NameBn,
    string NameEn,
    string DivisionBn,
    string DivisionEn,
    string[] Upazilas)
{
    public string DisplayName => $"{NameBn} ({NameEn})";
}

public static class BangladeshLocationData
{
    public static readonly IReadOnlyList<DistrictLocation> Districts = new List<DistrictLocation>
    {
        // ------------------ ঢাকা বিভাগ (Dhaka Division) [13] ------------------
        new("ঢাকা", "Dhaka", "ঢাকা", "Dhaka", new[]
        {
            "ধানমন্ডি (Dhanmondi)", "গুলশান (Gulshan)", "মিরপুর (Mirpur)", "উত্তরা (Uttara)", "সাভার (Savar)",
            "ধামরাই (Dhamrai)", "কেরানীগঞ্জ (Keraniganj)", "নবাবগঞ্জ (Nawabganj)", "দোহার (Dohar)"
        }),
        new("গাজীপুর", "Gazipur", "ঢাকা", "Dhaka", new[]
        {
            "গাজীপুর সদর (Gazipur Sadar)", "কালিয়াকৈর (Kaliakair)", "শ্রীপুর (Sreepur)", "কাপাসিয়া (Kapasia)", "কালীগঞ্জ (Kaliganj)"
        }),
        new("নারায়ণগঞ্জ", "Narayanganj", "ঢাকা", "Dhaka", new[]
        {
            "নারায়ণগঞ্জ সদর (Narayanganj Sadar)", "রূপগঞ্জ (Rupganj)", "সোনারগাঁও (Sonargaon)", "আড়াইহাজার (Araihazar)", "বন্দর (Bandar)"
        }),
        new("নরসিংদী", "Narsingdi", "ঢাকা", "Dhaka", new[]
        {
            "নরসিংদী সদর (Narsingdi Sadar)", "পলাশ (Palash)", "শিবপুর (Shibpur)", "রায়পুরা (Raipura)", "মনোহরদী (Monohardi)", "বেলাব (Belabo)"
        }),
        new("টাঙ্গাইল", "Tangail", "ঢাকা", "Dhaka", new[]
        {
            "টাঙ্গাইল সদর (Tangail Sadar)", "মির্জাপুর (Mirzapur)", "মধুপুর (Madhupur)", "ঘাটাইল (Ghatail)", "সখিপুর (Sakhipur)",
            "কালিহাতী (Kalihati)", "দেলদুয়ার (Delduar)", "গোপালপুর (Gopalpur)", "নাগরপুর (Nagarpur)", "বাসাইল (Basail)", "ধনবাড়ী (Dhanbari)", "ভূঞাপুর (Bhuapur)"
        }),
        new("কিশোরগঞ্জ", "Kishoreganj", "ঢাকা", "Dhaka", new[]
        {
            "কিশোরগঞ্জ সদর (Kishoreganj Sadar)", "ভৈরব (Bhairab)", "বাজিতপুর (Bajitpur)", "কটিয়াদী (Katiadi)", "করিমগঞ্জ (Karimganj)",
            "নিকলী (Nikli)", "তাড়াইল (Tarail)", "হোসেনপুর (Hossainpur)", "পাকুন্দিয়া (Pakundia)", "কুলিয়ারচর (Kuliarchar)", "ইটনা (Itna)", "মিঠামইন (Mithamain)", "অষ্টগ্রাম (Austagram)"
        }),
        new("মানিকগঞ্জ", "Manikganj", "ঢাকা", "Dhaka", new[]
        {
            "মানিকগঞ্জ সদর (Manikganj Sadar)", "সিংগাইর (Singair)", "শিবালয় (Shibalaya)", "সাটুরিয়া (Saturia)", "ঘিওর (Ghior)", "দৌলতপুর (Daulatpur)", "হরিরামপুর (Harirampur)"
        }),
        new("মুন্সীগঞ্জ", "Munshiganj", "ঢাকা", "Dhaka", new[]
        {
            "মুন্সীগঞ্জ সদর (Munshiganj Sadar)", "শ্রীনগর (Sreenagar)", "সিরাজদিখান (Sirajdikhan)", "লৌহজং (Louhajang)", "গজারিয়া (Gazaria)", "টংগিবাড়ী (Tongibari)"
        }),
        new("ফরিদপুর", "Faridpur", "ঢাকা", "Dhaka", new[]
        {
            "ফরিদপুর সদর (Faridpur Sadar)", "মধুখালী (Madhukhali)", "বোয়ালমারী (Boalmari)", "ভাঙ্গা (Bhanga)", "নগরকান্দা (Nagarkanda)",
            "আলফাডাঙ্গা (Alfadanga)", "সদরপুর (Sadarpur)", "চরভদ্রাসন (Charbhadrasan)", "সালথা (Saltha)"
        }),
        new("গোপালগঞ্জ", "Gopalganj", "ঢাকা", "Dhaka", new[]
        {
            "গোপালগঞ্জ সদর (Gopalganj Sadar)", "টুঙ্গিপাড়া (Tungipara)", "কোটালীপাড়া (Kotalipara)", "কাশিয়ানী (Kashiani)", "মুকসুদপুর (Muksudpur)"
        }),
        new("মাদারীপুর", "Madaripur", "ঢাকা", "Dhaka", new[]
        {
            "মাদারীপুর সদর (Madaripur Sadar)", "শিবচর (Shivchar)", "কালকিনি (Kalkini)", "রাজৈর (Rajoir)", "ডাসার (Dasar)"
        }),
        new("রাজবাড়ী", "Rajbari", "ঢাকা", "Dhaka", new[]
        {
            "রাজবাড়ী সদর (Rajbari Sadar)", "পাংশা (Pangsha)", "কালুখালী (Kalukhali)", "বালিয়াকান্দি (Baliakandi)", "গোয়ালন্দ (Goalanda)"
        }),
        new("শরীয়তপুর", "Shariatpur", "ঢাকা", "Dhaka", new[]
        {
            "শরীয়তপুর সদর (Shariatpur Sadar)", "জাজিরা (Zajira)", "নড়িয়া (Naria)", "ডামুড্যা (Damudya)", "ভেদরগঞ্জ (Bhedarganj)", "গোসাইরহাট (Gosairhat)"
        }),

        // ------------------ রাজশাহী বিভাগ (Rajshahi Division) [8] ------------------
        new("বগুড়া", "Bogura", "রাজশাহী", "Rajshahi", new[]
        {
            "বগুড়া সদর (Bogura Sadar)", "শিবগঞ্জ (Shibganj)", "শেরপুর (Sherpur)", "শাজাহানপুর (Shajahanpur)", "ধুনট (Dhunat)",
            "গাবতলী (Gabtali)", "সোনাতলা (Sonatala)", "সারিয়াকান্দি (Sariakandi)", "আদমদীঘি (Adamdighi)", "দুপচাঁচিয়া (Dupchanchia)", "কাহালু (Kahalu)", "নন্দীগ্রাম (Nandigram)"
        }),
        new("রাজশাহী", "Rajshahi", "রাজশাহী", "Rajshahi", new[]
        {
            "বোয়ালিয়া (Boalia)", "রাজশাহী সদর (Rajshahi Sadar)", "পবা (Paba)", "পুঠিয়া (Puthia)", "দুর্গাপুর (Durgapur)",
            "বাগমারা (Bagmara)", "চারঘাট (Charghat)", "বাঘা (Bagha)", "তানোর (Tanor)", "মোহনপুর (Mohanpur)", "গোদাগাড়ী (Godagari)"
        }),
        new("পাবনা", "Pabna", "রাজশাহী", "Rajshahi", new[]
        {
            "পাবনা সদর (Pabna Sadar)", "ঈশ্বরদী (Ishwardi)", "সাঁথিয়া (Santhia)", "সুজানগর (Sujanagar)", "চাটমোহর (Chatmohar)",
            "ফরিদপুর (Faridpur)", "বেড়া (Bera)", "ভাঙ্গুড়া (Bhangura)", "আটঘরিয়া (Atgharia)"
        }),
        new("সিরাজগঞ্জ", "Sirajganj", "রাজশাহী", "Rajshahi", new[]
        {
            "সিরাজগঞ্জ সদর (Sirajganj Sadar)", "শাহজাদপুর (Shahjadpur)", "উল্লাপাড়া (Ullapara)", "বেলকুচি (Belkuchi)", "রায়গঞ্জ (Raiganj)",
            "কামারখন্দ (Kamarkhanda)", "কাজীপুর (Kazipur)", "তাড়াশ (Tarash)", "চৌহালী (Chauhali)"
        }),
        new("নওগাঁ", "Naogaon", "রাজশাহী", "Rajshahi", new[]
        {
            "নওগাঁ সদর (Naogaon Sadar)", "মহাদেবপুর (Mohadevpur)", "পত্নীতলা (Patnitala)", "ধামইরহাট (Dhamoirhat)", "বদলগাছী (Badalgachhi)",
            "নিয়ামতপুর (Niamatpur)", "মান্দা (Manda)", "রানীনগর (Raninagar)", "আত্রাই (Atrai)", "পোরশা (Porsha)", "সাপাহার (Sapahar)"
        }),
        new("নাটোর", "Natore", "রাজশাহী", "Rajshahi", new[]
        {
            "নাটোর সদর (Natore Sadar)", "সিংড়া (Singra)", "বড়াইগ্রাম (Baraigram)", "গুরুদাসপুর (Gurudaspur)", "লালপুর (Lalpur)", "বাগাতিপাড়া (Bagatipara)", "নলডাঙ্গা (Naldanga)"
        }),
        new("জয়পুরহাট", "Joypurhat", "রাজশাহী", "Rajshahi", new[]
        {
            "জয়পুরহাট সদর (Joypurhat Sadar)", "পাঁচবিবি (Panchbibi)", "আক্কেলপুর (Akkelpur)", "কালাই (Kalai)", "ক্ষেতলাল (Khetlal)"
        }),
        new("চাঁপাইনবাবগঞ্জ", "Chapainawabganj", "রাজশাহী", "Rajshahi", new[]
        {
            "চাঁপাইনবাবগঞ্জ সদর (Chapainawabganj Sadar)", "শিবগঞ্জ (Shibganj)", "গোমস্তাপুর (Gomastapur)", "নাচোল (Nachole)", "ভোলাহাট (Bholahat)"
        }),

        // ------------------ রংপুর বিভাগ (Rangpur Division) [8] ------------------
        new("রংপুর", "Rangpur", "রংপুর", "Rangpur", new[]
        {
            "রংপুর সদর (Rangpur Sadar)", "মিঠাপুকুর (Mithapukur)", "পীরগঞ্জ (Pirganj)", "বদরগঞ্জ (Badarganj)", "পীরগাছা (Pirgachha)",
            "কাউনিয়া (Kaunia)", "গংগাচড়া (Gangachhara)", "তারাগঞ্জ (Taraganj)"
        }),
        new("দিনাজপুর", "Dinajpur", "রংপুর", "Rangpur", new[]
        {
            "দিনাজপুর সদর (Dinajpur Sadar)", "বিরল (Biral)", "বীরগঞ্জ (Birganj)", "বোচাগঞ্জ (Bochaganj)", "চিরিরবন্দর (Chirirbandar)",
            "ফুলবাড়ী (Phulbari)", "কাহারোল (Kaharole)", "খানসামা (Khansama)", "ঘোড়াঘাট (Ghoraghat)", "নবাবগঞ্জ (Nawabganj)", "পার্বতীপুর (Parbatipur)", "হাকিমপুর (Hili/Hakimpur)", "বিরামপুর (Birampur)"
        }),
        new("গাইবান্ধা", "Gaibandha", "রংপুর", "Rangpur", new[]
        {
            "গাইবান্ধা সদর (Gaibandha Sadar)", "গোবিন্দগঞ্জ (Gobindaganj)", "সাদুল্লাপুর (Sadullapur)", "পলাশবাড়ী (Palashbari)",
            "সুন্দরগঞ্জ (Sundarganj)", "সাঘাটা (Saghata)", "ফুলছড়ি (Fulchhari)"
        }),
        new("কুড়িগ্রাম", "Kurigram", "রংপুর", "Rangpur", new[]
        {
            "কুড়িগ্রাম সদর (Kurigram Sadar)", "নাগেশ্বরী (Nageshwari)", "ভূরুঙ্গামারী (Bhurungamari)", "উলিপুর (Ulipur)", "চিলমারী (Chilmari)",
            "রৌমারী (Roumari)", "চর রাজিবপুর (Char Rajibpur)", "ফুলবাড়ী (Phulbari)", "রাজারহাট (Rajarhat)"
        }),
        new("লালমনিরহাট", "Lalmonirhat", "রংপুর", "Rangpur", new[]
        {
            "লালমনিরহাট সদর (Lalmonirhat Sadar)", "আদিতমারী (Aditmari)", "কালীগঞ্জ (Kaliganj)", "হাতীবান্ধা (Hatibandha)", "পাটগ্রাম (Patgram)"
        }),
        new("নীলফামারী", "Nilphamari", "রংপুর", "Rangpur", new[]
        {
            "নীলফামারী সদর (Nilphamari Sadar)", "সৈয়দপুর (Saidpur)", "ডোমার (Domar)", "ডিমলা (Dimla)", "জলঢাকা (Jaldhaka)", "কিশোরগঞ্জ (Kishoreganj)"
        }),
        new("পঞ্চগড়", "Panchagarh", "রংপুর", "Rangpur", new[]
        {
            "পঞ্চগড় সদর (Panchagarh Sadar)", "দেবীগঞ্জ (Debiganj)", "বোদা (Boda)", "আটোয়ারী (Atwari)", "তেঁতুলিয়া (Tetulia)"
        }),
        new("ঠাকুরগাঁও", "Thakurgaon", "রংপুর", "Rangpur", new[]
        {
            "ঠাকুরগাঁও সদর (Thakurgaon Sadar)", "পীরগঞ্জ (Pirganj)", "বালিয়াডাঙ্গী (Baliadangi)", "রাণীশংকৈল (Ranisankail)", "হরিপুর (Haripur)"
        }),

        // ------------------ চট্টগ্রাম বিভাগ (Chattogram Division) [11] ------------------
        new("চট্টগ্রাম", "Chattogram", "চট্টগ্রাম", "Chattogram", new[]
        {
            "চট্টগ্রাম সদর (Chattogram Sadar)", "সীতাকুণ্ড (Sitakunda)", "মীরসরাই (Mirsharai)", "পটিয়া (Patiya)", "রাঙ্গুনিয়া (Rangunia)",
            "ফটিকছড়ি (Fatikchhari)", "হাটহাজারী (Hathazari)", "রাউজান (Raozan)", "বোয়ালখালী (Boalkhali)", "আনোয়ারা (Anwara)",
            "চন্দনাইশ (Chandanaish)", "লোহাগাড়া (Lohagara)", "সাতকানিয়া (Satkania)", "বাঁশখালী (Banshkhali)", "সন্দ্বীপ (Sandwip)", "কর্ণফুলী (Karnafuli)"
        }),
        new("কক্সবাজার", "Cox's Bazar", "চট্টগ্রাম", "Chattogram", new[]
        {
            "কক্সবাজার সদর (Cox's Bazar Sadar)", "রামু (Ramu)", "চকরিয়া (Chakaria)", "পেকুয়া (Pekua)", "উখিয়া (Ukhiya)", "টেকনাফ (Teknaf)", "মহেশখালী (Maheshkhali)", "কুতুবদিয়া (Kutubdia)"
        }),
        new("কুমিল্লা", "Cumilla", "চট্টগ্রাম", "Chattogram", new[]
        {
            "কুমিল্লা সদর (Cumilla Sadar)", "দাউদকান্দি (Daudkandi)", "চান্দিনা (Chandina)", "দেবিদ্বার (Debidwar)", "মুরাদনগর (Muradnagar)",
            "চৌদ্দগ্রাম (Chauddagram)", "লাকসাম (Laksam)", "বরুড়া (Barura)", "বুড়িচং (Burichang)", "ব্রাহ্মণপাড়া (Brahmanpara)",
            "মনোহরগঞ্জ (Monoharganj)", "তিতাস (Titas)", "মেঘনা (Meghna)", "হোমনা (Homna)", "লালমাই (Lalmai)"
        }),
        new("ব্রাহ্মণবাড়িয়া", "Brahmanbaria", "চট্টগ্রাম", "Chattogram", new[]
        {
            "ব্রাহ্মণবাড়িয়া সদর (Brahmanbaria Sadar)", "কসবা (Kasba)", "নবীনগর (Nabinagar)", "সরাইল (Sarail)", "আশুগঞ্জ (Ashuganj)",
            "আখাউড়া (Akhaura)", "বাঞ্ছারামপুর (Bancharampur)", "নাসিরনগর (Nasirnagar)", "বিজয়নগর (Bijoynagar)"
        }),
        new("চাঁদপুর", "Chandpur", "চট্টগ্রাম", "Chattogram", new[]
        {
            "চাঁদপুর সদর (Chandpur Sadar)", "হাজীগঞ্জ (Hajiganj)", "ফরিদগঞ্জ (Faridganj)", "মতলব উত্তর (Matlab North)",
            "মতলব দক্ষিণ (Matlab South)", "শাহরাস্তি (Shahrasti)", "কচুয়া (Kachua)", "হাইমচর (Haimchar)"
        }),
        new("নোয়াখালী", "Noakhali", "চট্টগ্রাম", "Chattogram", new[]
        {
            "নোয়াখালী সদর (Noakhali Sadar)", "বেগমগঞ্জ (Begumganj)", "চাটখিল (Chatkhil)", "সেনবাগ (Senbagh)", "সোনাইমুড়ী (Sonaimuri)",
            "কোম্পানীগঞ্জ (Companiganj)", "কবিরহাট (Kabirhat)", "সুবর্ণচর (Subarnachar)", "হাতিয়া (Hatiya)"
        }),
        new("ফেনী", "Feni", "চট্টগ্রাম", "Chattogram", new[]
        {
            "ফেনী সদর (Feni Sadar)", "দাগনভূঞা (Daganbhuiyan)", "সোনাগাজী (Sonagazi)", "ছাগলনাইয়া (Chhagalnaiya)", "পরশুরাম (Parshuram)", "ফুলগাজী (Fulgazi)"
        }),
        new("লক্ষ্মীপুর", "Lakshmipur", "চট্টগ্রাম", "Chattogram", new[]
        {
            "লক্ষ্মীপুর সদর (Lakshmipur Sadar)", "রায়পুর (Raipur)", "রামগঞ্জ (Ramganj)", "রামগতি (Ramgati)", "কমলনগর (Kamalnagar)"
        }),
        new("খাগড়াছড়ি", "Khagrachhari", "চট্টগ্রাম", "Chattogram", new[]
        {
            "খাগড়াছড়ি সদর (Khagrachhari Sadar)", "দীঘিনালা (Dighinala)", "পানছড়ি (Panchhari)", "মাটিরাঙ্গা (Matiranga)", "মহালছড়ি (Mahalchhari)", "মানিকছড়ি (Manikchhari)", "রামগড় (Ramgarh)", "গুইমারা (Guimara)"
        }),
        new("রাঙ্গামাটি", "Rangamati", "চট্টগ্রাম", "Chattogram", new[]
        {
            "রাঙ্গামাটি সদর (Rangamati Sadar)", "কাপ্তাই (Kaptai)", "বাঘাইছড়ি (Baghaichhari)", "বরকল (Barkal)", "নানিয়ারচর (Naniarchar)",
            "লংগদু (Langadu)", "বিলাইছড়ি (Bilaichhari)", "জুরাছড়ি (Jurachhari)", "কাউখালী (Kawkhali)", "রাজস্থলী (Rajasthali)"
        }),
        new("বান্দরবান", "Bandarban", "চট্টগ্রাম", "Chattogram", new[]
        {
            "বান্দরবান সদর (Bandarban Sadar)", "রোয়াংছড়ি (Rowangchhari)", "রুমা (Ruma)", "থানচি (Thanchi)", "লামা (Lama)", "আলীকদম (Alikadam)", "নাইক্ষ্যংছড়ি (Naikhongchhari)"
        }),

        // ------------------ খুলনা বিভাগ (Khulna Division) [10] ------------------
        new("খুলনা", "Khulna", "খুলনা", "Khulna", new[]
        {
            "খুলনা সদর (Khulna Sadar)", "ডুমুরিয়া (Dumuria)", "পাইকগাছা (Paikgachha)", "কয়রা (Koyra)", "রূপসা (Rupsha)",
            "তেরখাদা (Terokhada)", "দিঘলিয়া (Dighalia)", "বটিয়াঘাটা (Batiaghata)", "ফুলতলা (Phultala)", "দাকোপ (Dacope)"
        }),
        new("যশোর", "Jashore", "খুলনা", "Khulna", new[]
        {
            "যশোর সদর (Jashore Sadar)", "ঝিকরগাছা (Jhikargachha)", "শার্শা (Sharsha)", "মণিরামপুর (Manirampur)",
            "কেশবপুর (Keshabpur)", "অভয়নগর (Abhaynagar)", "বাঘারপাড়া (Bagherpara)", "চৌগাছা (Chaugachha)"
        }),
        new("কুষ্টিয়া", "Kushtia", "খুলনা", "Khulna", new[]
        {
            "কুষ্টিয়া সদর (Kushtia Sadar)", "কুমারখালী (Kumarkhali)", "মিরপুর (Mirpur)", "ভেড়ামারা (Bheramara)", "দৌলতপুর (Daulatpur)", "খোকসা (Khoksa)"
        }),
        new("বাগেরহাট", "Bagerhat", "খুলনা", "Khulna", new[]
        {
            "বাগেরহাট সদর (Bagerhat Sadar)", "মোংলা (Mongla)", "রামপাল (Rampal)", "মোরেলগঞ্জ (Morelganj)", "শরণখোলা (Sarankhola)",
            "কচুয়া (Kachua)", "ফকিরহাট (Fakirhat)", "মোল্লাহাট (Mollahat)", "চিতলমারী (Chitalmari)"
        }),
        new("চুয়াডাঙ্গা", "Chuadanga", "খুলনা", "Khulna", new[]
        {
            "চুয়াডাঙ্গা সদর (Chuadanga Sadar)", "আলমডাঙ্গা (Alamdanga)", "দামুড়হুদা (Damurhuda)", "জীবননগর (Jibannagar)"
        }),
        new("ঝিনাইদহ", "Jhenaidah", "খুলনা", "Khulna", new[]
        {
            "ঝিনাইদহ সদর (Jhenaidah Sadar)", "শৈলকুপা (Shailkupa)", "হরিণাকুণ্ডু (Harinakunda)", "কালীগঞ্জ (Kaliganj)", "কোটচাঁদপুর (Kotchandpur)", "মহেশপুর (Maheshpur)"
        }),
        new("মাগুরা", "Magura", "খুলনা", "Khulna", new[]
        {
            "মাগুরা সদর (Magura Sadar)", "শ্রীপুর (Sreepur)", "মহম্মদপুর (Mohammadpur)", "শালিখা (Shalikha)"
        }),
        new("মেহেরপুর", "Meherpur", "খুলনা", "Khulna", new[]
        {
            "মেহেরপুর সদর (Meherpur Sadar)", "গাংনী (Gangni)", "মুজিবনগর (Mujibnagar)"
        }),
        new("নড়াইল", "Narail", "খুলনা", "Khulna", new[]
        {
            "নড়াইল সদর (Narail Sadar)", "লোহাগড়া (Lohagara)", "কালিয়া (Kalia)"
        }),
        new("সাতক্ষীরা", "Satkhira", "খুলনা", "Khulna", new[]
        {
            "সাতক্ষীরা সদর (Satkhira Sadar)", "কলারোয়া (Kalaroa)", "তালা (Tala)", "আশাশুনি (Assasuni)", "দেবহাটা (Debhata)", "কালীগঞ্জ (Kaliganj)", "শ্যামনগর (Shyamnagar)"
        }),

        // ------------------ ময়মনসিংহ বিভাগ (Mymensingh Division) [4] ------------------
        new("ময়মনসিংহ", "Mymensingh", "ময়মনসিংহ", "Mymensingh", new[]
        {
            "ময়মনসিংহ সদর (Mymensingh Sadar)", "মুক্তাগাছা (Muktagachha)", "ত্রিশাল (Trishal)", "ফুলবাড়িয়া (Fulbaria)", "গৌরীপুর (Gauripur)",
            "ঈশ্বরগঞ্জ (Ishwarganj)", "নান্দাইল (Nandail)", "ফুলপুর (Phulpur)", "হালুয়াঘাট (Haluaghat)", "ধোবাউড়া (Dhobaura)",
            "গফরগাঁও (Gafargaon)", "ভালুকা (Bhaluka)", "তারাকান্দা (Tarakanda)"
        }),
        new("জামালপুর", "Jamalpur", "ময়মনসিংহ", "Mymensingh", new[]
        {
            "জামালপুর সদর (Jamalpur Sadar)", "সরিষাবাড়ী (Sarishabari)", "মেলান্দহ (Melandaha)", "ইসলামপুর (Islampur)",
            "মাদারগঞ্জ (Madarganj)", "দেওয়ানগঞ্জ (Dewanganj)", "বকশীগঞ্জ (Bakshiganj)"
        }),
        new("নেত্রকোণা", "Netrokona", "ময়মনসিংহ", "Mymensingh", new[]
        {
            "নেত্রকোণা সদর (Netrokona Sadar)", "কেন্দুয়া (Kendua)", "মদন (Madan)", "মোহনগঞ্জ (Mohanganj)", "দুর্গাপুর (Durgapur)",
            "কলমাকান্দা (Kalmakanda)", "বারহাট্টা (Barhatta)", "পূর্বধলা (Purbadhala)", "আটপাড়া (Atpara)", "খালিয়াজুড়ী (Khaliajuri)"
        }),
        new("শেরপুর", "Sherpur", "ময়মনসিংহ", "Mymensingh", new[]
        {
            "শেরপুর সদর (Sherpur Sadar)", "নালিতাবাড়ী (Nalitabari)", "নকলা (Nakla)", "ঝিনাইগাতী (Jhenaigati)", "শ্রীবরদী (Sreebardi)"
        }),

        // ------------------ বরিশাল বিভাগ (Barishal Division) [6] ------------------
        new("বরিশাল", "Barishal", "বরিশাল", "Barishal", new[]
        {
            "বরিশাল সদর (Barishal Sadar)", "বাকেরগঞ্জ (Bakerganj)", "বাবুগঞ্জ (Babuganj)", "উজিরপুর (Wazirpur)", "বানারীপাড়া (Banaripara)",
            "গৌরনদী (Gournadi)", "আগৈলঝাড়া (Agailjhara)", "মেহেন্দিগঞ্জ (Mehendiganj)", "মুলাদী (Muladi)", "হিজলা (Hizla)"
        }),
        new("বরগুনা", "Barguna", "বরিশাল", "Barishal", new[]
        {
            "বরগুনা সদর (Barguna Sadar)", "আমতলী (Amtali)", "তালতলী (Taltali)", "পাথরঘাটা (Patharghata)", "বেতাগী (Betagi)", "বামনা (Bamna)"
        }),
        new("ভোলা", "Bhola", "বরিশাল", "Barishal", new[]
        {
            "ভোলা সদর (Bhola Sadar)", "বোরহানউদ্দিন (Borhanuddin)", "দৌলতখান (Daulatkhan)", "লালমোহন (Lalmohan)",
            "চরফ্যাশন (Char Fasson)", "তজুমদ্দিন (Tazumuddin)", "মনপুরা (Monpura)"
        }),
        new("ঝালকাঠি", "Jhalokati", "বরিশাল", "Barishal", new[]
        {
            "ঝালকাঠি সদর (Jhalokati Sadar)", "নলছিটি (Nalchhiti)", "রাজাপুর (Rajapur)", "কাঁঠালিয়া (Kathalia)"
        }),
        new("পটুয়াখালী", "Patuakhali", "বরিশাল", "Barishal", new[]
        {
            "পটুয়াখালী সদর (Patuakhali Sadar)", "বাউফল (Bauphal)", "গলাচিপা (Galachipa)", "দশমিনা (Dashmina)",
            "কলাপাড়া (Kalapara)", "মির্জাগঞ্জ (Mirzaganj)", "দুমকি (Dumki)", "রাঙ্গাবালী (Rangabali)"
        }),
        new("পিরোজপুর", "Pirojpur", "বরিশাল", "Barishal", new[]
        {
            "পিরোজপুর সদর (Pirojpur Sadar)", "নাজিরপুর (Nazirpur)", "মঠবাড়িয়া (Mathbaria)", "ভান্ডারিয়া (Bhandaria)",
            "কাউখালী (Kawkhali)", "স্বরূপকাঠি/নেছারাবাদ (Nesarabad)", "ইন্দুরকানী (Indurkani)"
        }),

        // ------------------ সিলেট বিভাগ (Sylhet Division) [4] ------------------
        new("সিলেট", "Sylhet", "সিলেট", "Sylhet", new[]
        {
            "সিলেট সদর (Sylhet Sadar)", "ওসমানীনগর (Osmani Nagar)", "বিশ্বনাথ (Bishwanath)", "বালাগঞ্জ (Balaganj)", "ফেঞ্চুগঞ্জ (Fenchuganj)",
            "গোলাপগঞ্জ (Golapganj)", "বিয়ানীবাজার (Beanibazar)", "জকিগঞ্জ (Zakiganj)", "কানাইঘাট (Kanaighat)", "গোয়াইনঘাট (Gowainghat)",
            "জৈন্তাপুর (Jaintiapur)", "কোম্পানীগঞ্জ (Companiganj)", "দক্ষিণ সুরমা (Dakshin Surma)"
        }),
        new("মৌলভীবাজার", "Moulvibazar", "সিলেট", "Sylhet", new[]
        {
            "মৌলভীবাজার সদর (Moulvibazar Sadar)", "শ্রীমঙ্গল (Sreemangal)", "কমলগঞ্জ (Kamalganj)", "কুলাউড়া (Kulaura)", "জুড়ী (Juri)", "বড়লেখা (Barlekha)", "রাজনগর (Rajnagar)"
        }),
        new("হবিগঞ্জ", "Habiganj", "সিলেট", "Sylhet", new[]
        {
            "হবিগঞ্জ সদর (Habiganj Sadar)", "মাধবপুর (Madhabpur)", "চুনারুঘাট (Chunarughat)", "বাহুবল (Bahubal)", "নবীগঞ্জ (Nabiganj)",
            "বানিয়াচং (Baniachong)", "লাখাই (Lakhai)", "আজমিরীগঞ্জ (Ajmiriganj)", "শায়েস্তাগঞ্জ (Shayestaganj)"
        }),
        new("সুনামগঞ্জ", "Sunamganj", "সিলেট", "Sylhet", new[]
        {
            "সুনামগঞ্জ সদর (Sunamganj Sadar)", "ছাতক (Chhatak)", "জগন্নাথপুর (Jagannathpur)", "দোয়ারাবাজার (Dowarabazar)", "দিরাই (Dirai)",
            "শাল্লা (Shalla)", "ধর্মপাশা (Dharmapasha)", "জামালগঞ্জ (Jamalganj)", "তাহিরপুর (Tahirpur)", "বিশ্বম্ভরপুর (Bishwamvarpur)",
            "শান্তিগঞ্জ (Shantiganj)", "মধ্যনগর (Madhyanagar)"
        })
    };

    public static IReadOnlyList<string> GetAllDivisions() =>
        Districts.Select(d => $"{d.DivisionBn} ({d.DivisionEn})").Distinct().ToList();

    public static IReadOnlyList<DistrictLocation> GetDistrictsByDivision(string division) =>
        Districts.Where(d => d.DivisionBn.Equals(division, StringComparison.OrdinalIgnoreCase) ||
                             d.DivisionEn.Equals(division, StringComparison.OrdinalIgnoreCase) ||
                             $"{d.DivisionBn} ({d.DivisionEn})".Equals(division, StringComparison.OrdinalIgnoreCase))
                 .ToList();

    public static IReadOnlyList<string> GetAllDistrictDisplayNames() =>
        Districts.Select(d => d.DisplayName).OrderBy(n => n).ToList();

    public static IReadOnlyList<string> GetUpazilasForDistrict(string? districtDisplayName)
    {
        if (string.IsNullOrWhiteSpace(districtDisplayName))
            return Array.Empty<string>();

        var found = Districts.FirstOrDefault(d => 
            d.DisplayName.Equals(districtDisplayName, StringComparison.OrdinalIgnoreCase) ||
            d.NameBn.Equals(districtDisplayName, StringComparison.OrdinalIgnoreCase) ||
            d.NameEn.Equals(districtDisplayName, StringComparison.OrdinalIgnoreCase) ||
            districtDisplayName.StartsWith(d.NameBn, StringComparison.OrdinalIgnoreCase) ||
            districtDisplayName.Contains(d.NameEn, StringComparison.OrdinalIgnoreCase));

        return found?.Upazilas ?? Array.Empty<string>();
    }
}
