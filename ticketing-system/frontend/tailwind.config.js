/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,jsx}"],
  theme: {
    extend: {
      fontFamily: {
        sans: ["Outfit", "ui-sans-serif", "system-ui", "sans-serif"]
      },
      colors: {
        ink: {
          950: "#070B16",
          900: "#0B1224",
          800: "#121A32",
          700: "#1A2444"
        }
      },
      boxShadow: {
        glow: "0 0 40px rgba(34, 211, 238, 0.12)"
      }
    }
  },
  plugins: []
};
