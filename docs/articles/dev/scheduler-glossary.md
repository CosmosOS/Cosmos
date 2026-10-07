# Scheduler Glossary

Short background notes on the general scheduling concepts the [Scheduler](scheduler.md) article builds on. Each page explains the concept on its own, then closes with one line on how the Cosmos scheduler applies it. Replacing the policy itself is not a concept but a how-to: see [Writing a scheduler](../user/scheduler-plugging.md).

| Concept | Summary |
|---------|---------|
| [Preemption](sched-concepts/preemption.md) | The kernel takes the CPU away on an interrupt; threads never have to volunteer |
| [Virtual-time fair-share](sched-concepts/virtual-time-fair-share.md) | Weighted CPU shares, enforced by always running the thread whose virtual clock is furthest behind |
| [Interrupts at instruction boundaries](sched-concepts/instruction-boundary.md) | An interrupt lands after one instruction and before the next; nothing longer is atomic against it |
