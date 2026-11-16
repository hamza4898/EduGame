import { state } from "./core.js";

export function isTeacherAccount() {
    const role = String(state.currentUserRole).toLowerCase();
    return role === "teacher" || role === "2";
}

export function showWalletForCurrentRole() {
    const teacherPanel = document.getElementById("teacherWalletPanel");
    const studentPanel = document.getElementById("studentWalletPanel");
    const teacherWallet = isTeacherAccount();

    if (teacherPanel) teacherPanel.style.display = teacherWallet ? "flex" : "none";
    if (studentPanel) studentPanel.style.display = teacherWallet ? "none" : "flex";
}