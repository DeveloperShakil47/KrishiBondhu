# কৃষিবন্ধু ডিসিশন ইঞ্জিন | KrishiBondhu Decision Engine
### Secure Agricultural Trading and Farmer Decision Support Platform (Bangladesh)

[![.NET 10 LTS](https://img.shields.io/badge/.NET-10.0%20LTS-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor Interactive Server](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-172.16.102.123%3A5432-336791?logo=postgresql)](https://www.postgresql.org/)
[![EF Core 10](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://learn.microsoft.com/ef/core/)
[![Tests](https://img.shields.io/badge/Tests-21%20Passed%20(100%25)-brightgreen)](https://github.com/)

---

## 🌾 Overview / পরিচিতি

**KrishiBondhu Decision Engine (কৃষিবন্ধু ডিসিশন ইঞ্জিন)** is a production-grade, secure, and user-friendly agricultural trading and decision support platform engineered specifically for the agrarian ecosystem of Bangladesh.

### Core Objectives:
1. **Farmer Decision Support Engine**: Empowers farmers to evaluate competing buyer offers by calculating **True Net Selling Proceeds** after deducting transparent itemized costs (transport, bagging/packaging, loading/labor, and platform fees).
2. **Crop Profitability Clarity**: Strictly separates **Net Selling Proceeds** from **True Crop Profit** (accounting for agricultural input/production costs such as seed, fertilizer, irrigation, and field labor) so farmers never mistake gross proceeds for operating profit.
3. **Bangladeshi Weight Standardization**: Native support for **মণ (Mon / Maund)** and **কেজি (KG)** with automatic conversion ($1\text{ Mon} = 40\text{ KG}$).
4. **Digital Scale Weighing Verification**: Server-enforced double-entry weighing calculations ($\text{Net Weight} = \text{Gross Weight} - \text{Tare Weight}$) with joint digital signatures from both farmer and buyer or certified weighing operators.
5. **Direct Verified Payments**: Direct farmer payments via bKash, Nagad, Rocket, Bank Transfer, or Cash on Delivery. Financial safety guardrails require the farmer to independently verify receipt of funds before orders can be marked paid.
6. **Dispute Resolution & Auditability**: Formal mediation workflow for quality discrepancies, weight mismatches, or payment delays with append-only audit logging and tamper-proof agreed-terms order snapshots.

---

## 🏛️ Architectural Design

The platform adheres to **Clean Architecture** and **Modular Monolith** principles with strict dependency inversion:

```
KrishiBondhu.slnx
├── src/
│   ├── KrishiBondhu.Domain/           # Enterprise domain model, value objects, rules & enums (Zero dependencies)
│   ├── KrishiBondhu.Application/      # Use cases, DTOs, service contracts & repository interfaces
│   ├── KrishiBondhu.Infrastructure/   # PostgreSQL EF Core DbContext, Repositories, Unit of Work, Identity, Storage & Auditing
│   └── KrishiBondhu.Web/              # ASP.NET Core 10 Blazor Interactive Server UI with Bangla-First Theme
└── tests/
    ├── KrishiBondhu.UnitTests/        # Unit tests: Weights, Deductions, Net Proceeds, Order State Machine
    └── KrishiBondhu.IntegrationTests/ # Integration tests: Workflows, Snapshot Immutability, EF Core Transactions
```

### Domain Rules & Enforcements
- **Weight Consistency**: $\text{Gross Weight} > \text{Tare Weight}$; negative or zero tare weight rejected.
- **Financial Calculations**:
  $$\text{Gross Sale Value} = \text{Accepted Weight (KG)} \times \text{Agreed Price per KG}$$
  $$\text{Total Deductions} = \text{Transport} + \text{Packaging} + \text{Labor/Weighing} + \text{Platform Fee} + \text{Other}$$
  $$\text{Net Selling Proceeds} = \text{Gross Sale Value} - \text{Total Deductions}$$
  $$\text{True Crop Profit} = \text{Net Selling Proceeds} - \text{Farmer Production Cost}$$
  $$\text{Outstanding Balance} = \text{Agreed Total Price} - \text{Verified Paid Amount}$$
- **Order State Machine Transitions**:
  `Draft` $\rightarrow$ `Confirmed` $\rightarrow$ `Weighing` $\rightarrow$ `Dispatched` $\rightarrow$ `Delivered` $\rightarrow$ `PaymentPending` $\rightarrow$ `Completed` (or `Disputed` / `Cancelled`). Disputed orders cannot transition to `Completed` until formally resolved.

---

## 🗄️ Database Configuration

The application is connected to a remote PostgreSQL instance with EF Core Code First:

- **Host**: `172.16.102.123`
- **Port**: `5432`
- **Database**: `KrishiBondhuDb`
- **User**: `postgres`
- **Connection String** (`src/KrishiBondhu.Web/appsettings.json`):
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=172.16.102.123;Port=5432;Database=KrishiBondhuDb;User Id=postgres;Password=abcd123!"
  }
  ```

### Database Entities & Schema:
- **Identity & Profiles**: `ApplicationUser`, `FarmerProfile`, `FarmPlot`, `BuyerProfile`.
- **Trading & Negotiation**: `Crop`, `HarvestLot`, `BuyerOffer`, `OfferCost`, `BuyerDemand`.
- **Fulfillment & Verification**: `Order`, `WeighingRecord`, `PaymentRecord`, `DeliveryRecord`, `EvidenceRecord`.
- **Governance & Monitoring**: `DisputeCase`, `AuditLog`, `Notification`.

---

## 👥 Pre-Seeded Demo Accounts & Roles

When the web application boots, `DbInitializer` verifies schemas and seeds the following demo credentials (password: `Demo12345!` for all demo roles; Admin has dedicated master credentials):

| Role | Email | Password | Details |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin@krishibondhu.gov.bd` | `AdminSecurePassword@2026!` | Full platform oversight, audit log viewing, trade license verification |
| **Farmer** | `farmer.rahim@krishibondhu.gov.bd` | `Demo12345!` | Rahim Uddin (Bogura). 5.5 acres, Boro Paddy, Potato, Jute lots |
| **Buyer** | `buyer.tariq@krishibondhu.gov.bd` | `Demo12345!` | Tariqul Islam (Pran Agro Ltd). Verified buyer submitting offers |
| **Weighing Operator** | `scale.hasan@krishibondhu.gov.bd` | `Demo12345!` | Hasan Mahmud (Shibganj Scale Hub). Certified digital scale operator |
| **Support Staff** | `support.karim@krishibondhu.gov.bd` | `Demo12345!` | Karim Ullah. Dispute mediator and trade regulator |

> **Tip**: The login page (`/login`) includes interactive **One-Click Role Switcher** buttons to instantly log in as any role for end-to-end testing without typing credentials!

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Connectivity to PostgreSQL (`172.16.102.123:5432`)

### 1. Restore & Build
```powershell
dotnet restore KrishiBondhu.slnx
dotnet build KrishiBondhu.slnx
```

### 2. Run Database Migrations (if setting up fresh)
```powershell
dotnet ef database update --project src/KrishiBondhu.Infrastructure --startup-project src/KrishiBondhu.Web
```

### 3. Run the Test Suites
All 21 Unit and Integration tests run automatically:
```powershell
dotnet test KrishiBondhu.slnx
```

Test Results Breakdown:
- **`KrishiBondhu.UnitTests`**: 17 tests passed (Weight conversions, Tare validation, Net Proceeds, True Crop Profit, Order state machine transitions, Outstanding balance).
- **`KrishiBondhu.IntegrationTests`**: 4 tests passed (Lot creation, Offer comparison, Order creation with immutable snapshot, Payment verification guardrail).

### 4. Launch the Web Application
```powershell
dotnet run --project src/KrishiBondhu.Web
```
Open your browser and navigate to:
```
http://localhost:5030
```

---

## 📱 Platform Features & Pages

- **`/` (Landing Page)**: Hero banner, live agricultural commodity ticker, interactive Quick Decision Proceeds Calculator, step-by-step workflow overview.
- **`/login` & `/register`**: Bangla-first authentication with role selection (Farmer, Buyer, Operator) and 1-click demo switcher.
- **`/dashboard`**: Role-aware metrics and operational dashboard (Farmer sales stats, Buyer procurement stats, recent orders).
- **`/lots` & `/lots/{id}`**: Browse harvest lots, filter by crop/district, create new harvest lot with Mon/Kg conversions, and submit competitive buyer offers.
- **`/offers/compare/{lotId}` (The Decision Engine)**: Comprehensive side-by-side comparison of all buyer offers for a crop lot. Itemized breakdown of transport, packaging, labor, and platform fee deductions. Real-time True Crop Profit calculation. One-click offer acceptance with atomic order snapshot creation.
- **`/orders` & `/orders/{id}`**: End-to-end milestone timeline (`Draft` $\rightarrow$ `Completed`), immutable agreed terms, double-entry digital scale weighing input, direct payment submission (bKash/Nagad/Bank), and receipt verification.
- **`/buyer-demands`**: Public procurement notices posted by verified agro-processors and wholesalers.
- **`/disputes`**: Formal dispute filing and mediation portal.
- **`/admin`**: System audit logs, buyer trade license verification, and crop catalog administration.

---

## 🔒 Security & Data Integrity

1. **Immutable Order Terms Snapshot**: When an offer is accepted, all terms (unit price, agreed quantity, itemized deductions, delivery address) are serialized into the Order record as an immutable contract snapshot to prevent post-acceptance tampering.
2. **Double-Entry Weighing Guardrail**: Server independently verifies Gross and Tare weights entered from the scale.
3. **No Auto-Payment Verification**: Buyers submitting payment screenshots or transaction IDs cannot self-verify payments; farmers must explicitly confirm bank/wallet receipt.
4. **IDOR & Resource Authorization**: Farmers can only modify their own lots and orders; buyers can only view or adjust their own offers; only certified operators or order parties can record weights.
5. **Audit Logging**: All order creation, status transitions, weighing entries, payment submissions, and dispute escalations write to the `AuditLogs` table with UTC timestamp and actor identity.

---

## 🇧🇩 Localization

Built with dual-language support (`bn-BD` Bangla and `en-US` English) powered by `ILocalizationService`:
- Default locale: Bangla (বাংলা).
- Agricultural terms accurately mapped to traditional Bangladeshi farming conventions (মণ, কেজি, আড়ত, বোরো, আমন, পাট, চালান).
