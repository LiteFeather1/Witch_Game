﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CardUIPowerUp : CardUi, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image i_powerUp;
    [SerializeField] private TextMeshProUGUI t_cardName; 
    private Transform _originalParent;
    private PowerUp _powerUp;
    private Vector2 _velocity;
    private Quaternion _deriv;
    private bool _dragging;

    public PowerUp PowerUp => _powerUp;

    public Action<CardUIPowerUp> OnPickedUp { get; set; }
    public Action<CardUIPowerUp> OnUsed { get; set; }
    public Action<CardUIPowerUp> OnDropped { get; set; }
    public Action<PowerUp> OnShowDescription { get; set; }

    public void Update()
    {
        if (_dragging)
        {
            var delta = Time.deltaTime;
            transform.position = Vector2.SmoothDamp(transform.position, Input.mousePosition, ref _velocity, delta * 6f);
            var zRotation = Mathf.Lerp(0f, 24f, Mathf.Abs(_velocity.x) / 1200f) * Mathf.Sign(_velocity.x);
            var to = Quaternion.Euler(0f, 0f, -zRotation);
            transform.localRotation = SmoothDamp(transform.localRotation, to, ref _deriv, delta * 128f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnShowDescription?.Invoke(_powerUp);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnPickedUp?.Invoke(this);
            _dragging = true;
            _originalParent = transform.parent;
            transform.SetParent(transform.parent.parent);
            _canvasGroup.blocksRaycasts = false;
            OnCardUnHovered?.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _dragging = false;
            transform.SetParent(_originalParent);
            _canvasGroup.blocksRaycasts = true;
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            OnDropped?.Invoke(this);
        }
    }

    public void SetPowerUp(PowerUp powerUp)
    {
        _powerUp = powerUp;
        i_powerUp.sprite = powerUp.Icon;
        i_card.color = powerUp.RarityColour;
        t_cardName.text = powerUp.Name;
    }

    public void Used()
    {
        OnUsed?.Invoke(this);
    }

    public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
    {
        if (Time.deltaTime < Mathf.Epsilon) return rot;
        // account for double-cover
        var Dot = Quaternion.Dot(rot, target);
        var Multi = Dot > 0f ? 1f : -1f;
        target.x *= Multi;
        target.y *= Multi;
        target.z *= Multi;
        target.w *= Multi;
        // smooth damp (nlerp approx)
        var Result = new Vector4(
            Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
            Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
            Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
            Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
        ).normalized;

        // ensure deriv is tangent
        var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
        deriv.x -= derivError.x;
        deriv.y -= derivError.y;
        deriv.z -= derivError.z;
        deriv.w -= derivError.w;

        return new Quaternion(Result.x, Result.y, Result.z, Result.w);
    }
}
