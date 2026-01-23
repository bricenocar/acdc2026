# DataBlocks at ACDC: Building an Intelligent In-Game Experience

For the **Arctic Cloud Developer Challenge (ACDC)**, our team **DataBlocks** is focusing on building an **intelligent, cloud-backed in-game experience**, rather than a traditional external dashboard or backend-only solution.

The core idea is simple:

> Bring logic, AI, and data **directly into the game**, where players and administrators interact with it in real time.

Below is an overview of what we’re building and the technologies involved.

---

## In-Game Minigame Logic

We’re implementing a **custom minigame experience** inside the game, with its own dedicated logic layer. This includes:

* Points and in-game currency
* Rewards and progression
* Player state and session tracking

Instead of hardcoding rules, the game logic is **data-driven**, allowing us to evolve mechanics and balance gameplay without redeploying or modifying the game itself.

---

## Real-Time Game Administration

One of the key features is a dedicated **game administrator role**.

An admin can make live changes to an ongoing game session, such as:

* Switching between **day and night**
* Assigning **weapons, items, or points** to specific players
* Influencing the session dynamically without restarting it

This turns game administration into a **real-time operational experience**, similar to managing a live service rather than a static game world.

---

## Player & Agent Communication via Bot Framework Skills (or maybe Agents SDK :))

Communication between players, the game, and AI agents is handled using a **Bot Framework skill**.

This approach allows us to:

* Maintain a clean separation between game logic and conversational logic
* Reuse agents across different scenarios
* Enable real-time interaction between players and intelligent services

From an architectural perspective, this provides a **scalable and extensible integration model**.

---

## Dataverse as the Operational Data Store

We’re using **Dataverse** as the primary database for operational data, including:

* Game sessions
* Player profiles
* Points, currency, and rewards
* Admin actions and game state

Dataverse gives us a **structured, secure, and relational data model** that fits well with transactional game data and session-based logic.

---

## Telemetry at Scale with Data Lake

All in-game telemetry is streamed to a **data lake**.

This includes events such as:

* Player deaths and kills
* Building actions
* Monster spawns and movement
* Environmental changes

The goal is to capture **everything that happens in the game**, at scale, without impacting gameplay performance.

This telemetry forms the foundation for analytics, insights, and AI-driven exploration.

---

## AI Capabilities Embedded in Gameplay

AI is not a side feature — it’s embedded directly into gameplay. We’re using AI in three main scenarios.

### 1. Game Helper Bot

Players can interact with a **game helper bot** through chat.

Example prompts:

* "Build a fortress"
* "Build a wall around me"
* "Create a safe zone"

The agent translates these requests into **Minecraft commands** that can be executed directly in the game, taking into account:

* The player’s available points or currency
* Game rules and constraints
* Current game state

The goal is to reduce friction and let players focus on **creativity rather than command syntax**.

---

### 2. Player Prompt Improver

Players don’t always write perfect prompts — and that’s expected.

We use an **LLM-based prompt improver** that:

* Takes short or vague player input
* Expands and adapts it to the Minecraft context
* Produces a richer, more precise prompt for the helper bot

This improves command quality and consistency without forcing players to "learn how to talk to AI".

---

### 3. Data Insights Agent

Finally, we’re building a **data insights agent** on top of the telemetry stored in the data lake.

This agent can answer questions such as:

* "What caused most player deaths in this session?"
* "Which areas had the highest monster activity?"
* "How did player behavior change over time?"

Instead of exporting data to external tools, insights are accessible through **natural language**, directly tied to the game session. itself.


# Governance Foundations in the DataBlocks Solution

From the start, governance was treated as a **design constraint**, not an afterthought. Because our solution combines AI agents, Power Platform, and a live game integration, we needed clear boundaries around **risk, permissions, and visibility**.

This document describes the governance work implemented so far.

---

## Environment Strategy: Risk-Based Zoning

We defined **five environment groups**, based on risk level and intended usage:

### Green Zone

* Intended for low-risk experimentation and learning
* Very limited permissions
* Agents cannot be shared
* Minimal set of connectors available

### Yellow Zone

* Intended for POCs and internal departmental agents
* Agents can be shared and published
* Usage limited to a smaller group of users
* Expanded but still controlled connector set

### Red Zone (DEV / TEST / PROD)

* Considered **high-risk environments by design**
* Broader permissions and richer connector access
* Intended for agents and applications used by many users
* DEV, TEST, and PROD are separated to ensure isolation while maintaining governance

This zoning model makes it explicit **where certain types of solutions are allowed to live** and what risk level they operate under.

---

## Data Loss Prevention (DLP) Policies

Each zone has a dedicated **DLP policy**, aligned with its risk profile.

### Green Zone DLP

* Very limited connector availability
* No sharing of agents
* Designed to prevent accidental data exposure

### Yellow Zone DLP

* Supports publishing and sharing of agents
* Expanded connector set
* Intended for controlled internal usage and experimentation

### Red Zone DLP

* Broad sharing allowed across the organization
* Expanded connector availability
* Intended for production-grade solutions

DLP policies ensure flexibility without compromising control.

---

## Red Zone Configuration for the Minecraft Integration

The **Minecraft integration** is hosted in the **Red Zone**, which is appropriate given its real-time interaction model, AI agents, and external API usage.

The following connectors are explicitly enabled through the Red Zone DLP policy:

* **DocumentCorePack (Dataverse / mscrm connector)**
  Used to store game sessions, player data, points, and game state.

* **Direct Line channel in Copilot Studio**
  Required to develop the Bot Framework skill and connect it to agents via Direct Line.

* **Public website knowledge sources in Copilot Studio**
  Used to retrieve Minecraft commands and related information from public web sources.

All connectors are enabled intentionally and tied to a clear functional requirement.

---

## Visibility and Risk Detection with DSPM for AI

We use **DSPM for AI** to maintain visibility across all agents.

This allows us to:

* Identify risky agents
* Detect risky behaviors
* Understand sharing scope and usage patterns

This provides governance at the **agent level**, not just the environment level.

---

## Access Controls and AI-Specific Protections

To reduce exposure and misuse:

* A **Conditional Access policy** blocks access to agents classified as risky
* A **prompt policy** is defined to mitigate **prompt injection attacks**

These controls add guardrails even when agents are broadly shared.

---

## Network Visibility with Global Secure Access

Using **Global Secure Access**, traffic logs related to agent activity and outbound requests are collected.

This enables:

* Alerting on suspicious behavior
* Enforcement of allowed or blocked web destinations
* Detection of unexpected traffic patterns

This adds a network-level layer to the governance model.

---

## Auditing and Bot Activity Tracking

Audit logging is enabled, and **bot activity logs** are available.

Key governance-relevant events tracked include:

* Bot created or deleted
* Bot published
* Bot shared
* **Authentication mechanism updated** (critical governance signal)