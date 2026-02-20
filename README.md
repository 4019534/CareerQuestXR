# CareerQuestXR 

> **Mapping Your Future in Extended Reality (XR) through Virtual Aptitude Testing**

CareerQuestXR is a proof-of-concept Extended Reality (XR) aptitude assessment system developed as part of an MSc Computer Science thesis at the University of the Western Cape (UWC). It addresses persistently high first-year dropout rates in South African higher education; driven by career misalignment and inadequate guidance, by delivering immersive, performance-based aptitude assessments in a standalone VR environment. Below is the link to the current apk of thr prototype developed for this research:

https://drive.google.com/file/d/1GjwbksUuh9W3O4o9moda-acib-rSPvJW/view?usp=share_link

---

## What It Does

CareerQuestXR evaluates students across four multidimensional competency domains:

| Domain | Constructs Measured |
|---|---|
| **Intellectual (I)** | Verbal comprehension, numerical reasoning |
| **Cognitive (C)** | Working memory, spatial processing, reaction time |
| **Psychological (P)** | Stress management, emotional regulation |
| **Behavioural (B)** | Persistence, adaptability, hint utilisation |

Participants complete performance-based VR tasks while the system captures objective behavioural metrics (accuracy, task completion time, hint usage, and interaction patterns) with millisecond precision. These metrics are integrated with Holland's **RIASEC** vocational interest model and the **Big Five** personality framework to compute an **Academic Alignment Index (AAI)** score, which ranks student-program fit across 21 selected UWC academic programs.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Game Engine | Unity 6000.0.34f1 LTS |
| Programming Language | C# (.NET Standard 2.1 Framework) |
| VR Platform | Meta Quest 3S (128GB) |
| VR SDK | Meta XR SDK |
| UI Framework | Unity UI Toolkit |
| Data Export | JSON, CSV |

---

## Assessment Tasks

- **Verbal Comprehension Test** — Reading comprehension and verbal reasoning
- **Mental Maths Test** — Numerical reasoning under time pressure
- **Memory Recall Test** — Working memory sequence recall
- **Reaction Timer Test** — Cognitive processing speed
- **Navigation Grid** — Spatial processing and problem-solving
- **Psychological Evaluation Test** — Stress response and emotional regulation (combined with self-report surveys and heart rate data)

---

## How the Scoring Works

1. Raw task performance metrics are normalised into domain scores (I, C, P, B)
2. Domain scores are weighted and combined into a composite profile
3. The RIASEC Interest Profiler and TIPI Big Five survey results are layered in
4. An **Academic Readiness Threshold** pre-filter is applied to the Intellectual and Cognitive domain scores (≥40%)
5. The **AAI** ranks all 21 academic programs by student-program fit, producing a final recommendation report of the top 5 most suitable academic stream recommendations for the participant

---

## Repository Structure

```
Assets/
├── Scripts/
│   ├── SciFi/
│   │   ├── KeypadQuiz/               # Numerical reasoning task 2 logic
│   │   ├── MathQuiz/                 # Numerical reasoning task 1 logic
│   │   └── Nav/                      # Navigation grid (spatial reasoning)
│   ├── SpecialSkills/Cogs/           # Cognitive domain tasks (memory, reaction time, puzzles)
│   ├── Psche/                        # Psychological & behavioural domain (TIPI, personality profiler)
│   ├── Stats/                        # AAI scoring engine, career mapping & results UI
│   ├── MainMenu/                     # Scene and stats management
│   ├── CreateSaveUser/               # User authentication
│   ├── StartingScene/                # Starting screen logic
│   ├── Com/                          # Shared utility scripts
│   └── all_/                         # Verbal comprehension task logic
└── StreamingAssets/
    └── Configs/
        ├── BigFiveConfig.json        # Big Five personality framework config
        ├── WeightsConfig.json        # Domain weighting config
        ├── CareerFieldDB.json        # Academic program & career field database
        └── generate_career_report.py # Academic Stream report generation script
```

> Note: This repository contains the prototype codebase developed for a proof-of-concept study (N=15). It is not production-ready. Predictive validity has not yet been established and requires longitudinal validation.

---

## Dependencies

### Package Manager
| Package | Version | Purpose |
|---|---|---|
| Meta XR SDK All | 77.0.0 | Core VR layer for Meta Quest 3S |
| XR Interaction Toolkit | 3.2.0 | Controller input and object interactions |
| XR Management | 4.5.1 | XR plugin management |
| OpenXR | 1.14.3 | Cross-platform XR support |
| Universal Render Pipeline | 17.0.3 | Rendering pipeline |
| Input System | 1.11.2 | Controller and input handling |
| Newtonsoft JSON | 3.2.1 | JSON config parsing |
| AI Navigation | 2.0.5 | NavMesh for navigation grid task |
| Unity UI (uGUI) | 2.0.0 | In-app UI elements |
| NuGetForUnity | latest | .NET package management within Unity |

### Third-Party Assets (Unity Asset Store)
| Asset | Version | Author | Purpose |
|---|---|---|---|
| Simple Boids (Flocks of Birds, Fish and Insects) | 1.1.1 | Nick Veselov #NVJOB | Ambient environmental effects |
| Keypad FREE | 1.0.1 | Navarone | Physical keypad interaction |
| LeanTween | 2.51 | Dented Pixel | UI and object animations |
| Radial Menu Framework | 1.0 | Brett Gregory | Radial menu navigation |
| RPG & Fantasy Mobile GUI with Source Files | 1.0 | bonk! | UI styling and interface elements |
| School Assets | 1.0 | A.R.S\|T. | Environmental 3D assets |
| Sci-Fi Construction Kit (Modular) | 1.1.0 | Sickhead Games | SciFi environment 3D assets |
| Visual Keyboard | 1.0.0 | Martysh | In-VR keyboard input |

---

## Research Context

This prototype was developed as part of an MSc Computer Science thesis at the **University of the Western Cape**, supervised by **Dr. Andre Henney** and **Prof. Maria Florence**.

**Research Questions addressed:**
- **RQ1 (Operational Functionality):** Can CareerQuestXR generate differentiated multidimensional profiles across different skill levels, and offer preliminary evidence of construct validity and discriminating capacity?
- **RQ2 (Multidimensional Profiling):** Are the four domains (Intellectual, Cognitive, Psychological, Behavioural) contributing distinct and non-redundant information to the student’s multidimensional profile, providing tentative evidence of domain distinctness?
- **RQ3 (Academic Stream Recommendation Framework):** Will the system be able to produce relevant academic stream recommendations using the Academic Alignment Index (AAI) model, that compiles the participant’s domain scores, RIASEC fit, and personality alignment (Big Five) into a single profile, thus finding early stage proof of content validity and person-environment fit theory?

**Key findings (N=15: purposive sampling):**
- Full task completion with clear performance differentiation across all participants
- Moderate inter-domain independence (mean |r| = 0.589), revealing a two-construct model (I: Academic Capability vs. C-P-B: Self-Regulation Capacity)
- Systematic, theoretically coherent program recommendations via AAI scores
- Strong behavioural-performance correlation (r = +0.907), providing preliminary evidence for VR's ecological validity

---

## Future Development

Future development is guided by two complementary priorities: methodological strengthening and technical expansion.

Methodological priorities include conducting larger-scale studies (N ≥ 100) to enable confirmatory factor analysis and formal psychometric testing, and longitudinal tracking (3–5 years) to establish whether AAI scores and domain profiles meaningfully predict real-world academic outcomes such as retention, GPA, and degree completion. Measurement refinements are also needed, i.e., replacing manual heart rate reporting with continuous biometric monitoring, substituting the psychological instruments that were developed by the primary researcher with validated measures (e.g., STAI), and applying adaptive testing algorithms to eliminate the observed ceiling effects.

Technical enhancements include expanding domain coverage with additional validated instruments (e.g., DAT, NBTs, Grit Scale), developing industry-specific assessment modules for healthcare, STEM, and language fields, and extending recommendations beyond academic programs to specific career roles using RIASEC for field matching and Big Five for role differentiation. Longer-term plans explore AR and MR modalities, AI-powered behavioural analysis, and wearable biometric integration.

The study further proposes an institutional partnership model, contingent on longitudinal validation. This will include partnerships with NSFAS for risk-stratified funding allocation, corporate learnership pre-screening models, and phased national scaling across South African universities and TVET colleges. However, any such deployment requires comprehensive fairness audits, expert validation, and ethical governance frameworks before implementation.

---

## License

This project was developed for academic research purposes. Please contact the author before reuse or redistribution.
