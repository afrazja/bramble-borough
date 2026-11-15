# Prototype verification

## Gameplay

Unity batch-mode compilation and gameplay verification passed under Unity 6000.3.28f1, then passed again after upgrading the project to 6000.6.9f1.

Verified invalid pad/type rejection, exact purchase costs, occupied-pad rejection, insufficient funds, no overlapping waves, an undefended loss, a viable three-wave win, terminal state stability, and a fresh starting state. The winning simulation defeated all 36 enemies with 10 lives remaining.

## Windows smoke test

The initial Windows player built successfully under 6000.3.28f1 and was opened on the desktop. Visually checked the full landscape layout. Mouse testing verified selecting the Dew Keeper, placing it, amber changing from 148 to 63, starting wave two, visible enemies and health bars, pause keeping enemy positions and resources unchanged, and restart restoring 180 amber, 10 lives, wave zero, and empty pads.

The procedural artwork is an initial functional pass. Audio, sprite animation, final illustrations, accessibility options, and difficulty polish are not complete.

## Target builds

Web and Android verification results will be recorded after those builds finish. No phone was connected during the initial device check, so no physical-device testing has been performed.
