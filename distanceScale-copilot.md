---
**For calibration tips or code details, see the main README or ask Copilot!**

---| **Tomahawk**| `0.941505`  | 228.59     | -1 cm      | ~0.000005                   |
| **Cobra** | `0.985970`    | 228.60     | 0 cm       | ~0.000010                   |

### Precision Ranges (Tomahawk Example)
From experimental data, we found a very narrow sensitivity range:
- `0.941502` -> 228.58m (-2cm)
- `0.941511` -> 228.64m (+4cm)

To hit **228.60m (0cm)**, the estimated ideal scale is **`0.941505`**.
This demonstrates that a change of just **0.000009** can shift the landing point by **6cm**.

### Rule of Ranges (Bracketing Methodology)
**ALWAYS** use the Bracketing method for final precision tuning:
1.  **Find the Low Bound**: A scale value that results in a slight undershoot (e.g., -2cm).
2.  **Find the High Bound**: A scale value that results in a slight overshoot (e.g., +4cm).
3.  **Interpolate**: Calculate the exact value between them.
    - *Example*: If Range is 6cm, and you need +2cm from Low, add 1/3 of the scale difference to the Low scale.
    - **DO NOT GUESS**. Use the data to find the mathematical zero point.
