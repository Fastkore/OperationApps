from django.urls import path
from . import views

urlpatterns = [
    path('', views.operations_list, name='operations_list'),
    path('<int:operation_id>/', views.operation_detail, name='operation_detail'),
    path('<int:operation_id>/update_status/', views.update_status, name='update_status'),
]
