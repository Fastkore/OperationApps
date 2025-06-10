import requests
from django.shortcuts import render, redirect
from django.views.decorators.http import require_POST
from django.contrib.auth import authenticate, login, logout
from django.contrib.auth.decorators import login_required
from django.contrib import messages

ERP_BASE_URL = "https://erp.example.com/api/operations"
DUMMY_TOKEN = "demo-token"


def user_login(request):
    if request.method == "POST":
        username = request.POST.get("username")
        password = request.POST.get("password")
        user = authenticate(request, username=username, password=password)
        if user is not None:
            login(request, user)
            # store dummy token in session
            request.session["token"] = DUMMY_TOKEN
            return redirect("operations_list")
        messages.error(request, "Invalid credentials")
    return render(request, "login.html")


def user_logout(request):
    logout(request)
    return redirect("login")


@login_required
def operations_list(request):
    token = request.session.get("token", DUMMY_TOKEN)
    status = request.GET.get("status")
    headers = {"Authorization": f"Token {token}"}
    params = {}
    if status:
        params["status"] = status
    try:
        resp = requests.get(ERP_BASE_URL, headers=headers, params=params)
        resp.raise_for_status()
        operations = resp.json()
    except Exception:
        operations = []
    context = {
        "operations": operations,
        "status_filter": status or "",
    }
    return render(request, "operations_list.html", context)


@login_required
def operation_detail(request, operation_id):
    token = request.session.get("token", DUMMY_TOKEN)
    headers = {"Authorization": f"Token {token}"}
    try:
        resp = requests.get(f"{ERP_BASE_URL}/{operation_id}", headers=headers)
        resp.raise_for_status()
        operation = resp.json()
    except Exception:
        operation = {}
    return render(request, "operation_detail.html", {"operation": operation})


@login_required
@require_POST
def update_status(request, operation_id):
    token = request.session.get("token", DUMMY_TOKEN)
    headers = {"Authorization": f"Token {token}"}
    status = request.POST.get("status")
    try:
        requests.patch(f"{ERP_BASE_URL}/{operation_id}/", headers=headers, json={"status": status})
    except Exception:
        pass
    return redirect("operation_detail", operation_id=operation_id)
