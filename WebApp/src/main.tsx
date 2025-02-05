import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import Home from "./Home";
import PerftPage from "./Perft";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        {/* Define your routes here */}
        <Route path="/" element={<Home />} /> {/* Home page route */}
        <Route path="/perft/:id" element={<PerftPage />} />{" "}
        {/* Perft page with dynamic id */}
      </Routes>
    </BrowserRouter>
  </StrictMode>
);
